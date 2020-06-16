using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Net.NetworkInformation;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace GetNasaImages
{
	class Program
	{
		public static IConfigurationRoot configuration;
		public static INasaService nasaService;

		static string filePattern = "Nasa-APOD-##DATETIME##-##QUALITY##-##TITLE##.jpg";

		static void Main(string[] args)
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			loopToPast((args.Length >= 1) ? IsInteger(args[0]) : 1);
		}

		private static void ConfigureServices(ServiceCollection serviceCollection)
		{
			configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
			.AddJsonFile("appsettings.json", false)
			.Build();

			serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

			nasaService = new NasaService(configuration["NasaApiKey"]);

			serviceCollection.AddSingleton<INasaService>(nasaService);
		}

		static Int32? IsInteger(string i)
		{
			try
			{
				return Convert.ToInt32(i);
			}
			catch
			{
				return null;
			}
		}

		static void loopToPast(int? days)
		{
			DateTime? dt = DateTime.Now;
			int count = 0;

			while (count != days)
			{
				NasaAPOD apod = nasaService.getAPOD(false, dt);
				SaveAPOD(apod, dt);

				dt = dt?.AddDays(-1);
				count++;
			}
		}

		static void SaveAPOD(NasaAPOD apod, DateTime? dt)
		{
			if (apod != null)
			{
				if (apod.media_type == "image")
				{
					if (!Directory.Exists(@".\images\"))
						Directory.CreateDirectory(@".\images\");

					if (!Directory.Exists(@".\images\SD\"))
						Directory.CreateDirectory(@".\images\SD\");

					if (!Directory.Exists(@".\images\HD\"))
						Directory.CreateDirectory(@".\images\HD\");

					using (WebClient client = new WebClient())
					{
						if (!string.IsNullOrEmpty(apod.url))
						{
							string fileName = filePattern.Replace("##DATETIME##", (dt ?? DateTime.Now).ToString("yyyy-MM-dd"))
														 .Replace("##QUALITY##", "SD")
														 .Replace("##TITLE##", normalizeName(apod.title));
							client.DownloadFile(new Uri(apod.url), @".\images\SD\" + fileName);
						}

						if (!string.IsNullOrEmpty(apod.hdurl))
						{
							string fileName = filePattern.Replace("##DATETIME##", (dt ?? DateTime.Now).ToString("yyyy-MM-dd"))
														 .Replace("##QUALITY##", "HD")
														 .Replace("##TITLE##", normalizeName(apod.title));
							client.DownloadFile(new Uri(apod.hdurl), @".\images\HD\" + fileName);
						}
					}
				}
				else
				{
					Console.WriteLine(string.Format("NASA have published a {1} at {0}.\nBrowse '{3}' at {2}.\n\n", (dt ?? DateTime.Now).ToString("yyyy-MM-dd"), apod.media_type, apod.url, apod.title));
				}
			}
			else
			{
				Console.WriteLine(string.Format("NASA do not have published image at {0}.\n\n", (dt ?? DateTime.Now).ToString("yyyy-MM-dd")));
			}
		}

		static string normalizeName(string name)
		{
			return name.Replace(" ", "-").Replace("?", "").Replace("\\", "").ToUpper();
		}
	}
}
