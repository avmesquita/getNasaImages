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

		/// <summary>
		/// Main routine
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			//IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

			loopToPast((args.Length >= 1) ? IsInteger(args[0]) : 1);
		}

		/// <summary>
		/// .net core pattern
		/// </summary>
		/// <param name="serviceCollection"></param>
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

		/// <summary>
		/// Simple validation
		/// </summary>
		/// <param name="i"></param>
		/// <returns></returns>
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

		/// <summary>
		/// Main loop to get pictures
		/// </summary>
		/// <param name="days"></param>
		static void loopToPast(int? days)
		{
			DateTime? dt = DateTime.Now;
			int count = 0;

			StringBuilder html = new StringBuilder();
			html.AppendLine("<HTML>");
			html.AppendLine("  <HEAD>");
			html.AppendLine("     <TITLE>NASA :: Astronomic Picture of the Day</TITLE>");
			html.AppendLine("     <meta charset='UTF-8'>");
			html.AppendLine("     <meta name='description' content='Comemorative app to NASA Astronomic Picture of the Day 25 years'>");
			html.AppendLine("     <meta name='keywords' content='NASA, APOD, Astronomic, Picture, Day'>");
			html.AppendLine("     <meta name='author' content='NASA'>");
			html.AppendLine("     <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
			html.AppendLine("    <STYLE>");
			html.AppendLine("    .nasaDay{ padding: 20px; } ");
			html.AppendLine("    .nasaDate{ font-weight:bold; } ");
			html.AppendLine("    .nasaTitle{ font-weight:bold; } ");
			html.AppendLine("    .nasaExplanation{ text-align: justify; text-justify: inter - word; padding-top: 10px; padding-bottom: 10px; } ");
			html.AppendLine("    .nasaCopyright{ font-style:italic; } ");
			html.AppendLine("    .nasaImage{} ");
			html.AppendLine("    .nasaPic{ width:100% } ");
			html.AppendLine("    .nasaVideo{} ");
			html.AppendLine("    </STYLE>");

			html.AppendLine("  </HEAD>");
			html.AppendLine("  <BODY>");

			while (count != days)
			{
				NasaAPOD apod = nasaService.getAPOD(false, dt);
				string htmlSection = SaveAPOD(apod, dt);

				html.AppendLine(htmlSection);

				dt = dt?.AddDays(-1);
				count++;
			}
			html.AppendLine("  </BODY>");
			html.AppendLine("</HTML>");
			System.IO.File.WriteAllText(@".\NasaAPOD.html", html.ToString());
		}

		/// <summary>
		/// Save APOD images
		/// </summary>
		/// <param name="apod"></param>
		/// <param name="dt"></param>
		static string SaveAPOD(NasaAPOD apod, DateTime? dt, bool htmlLoadImagesFromInternet = false)
		{
			string html = string.Empty;

			if (apod != null)
			{
				html += "  <div class='nasaDay'>";

				string lowimg = string.Empty;
				string hiresimg = string.Empty;

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
							lowimg = @".\images\SD\" + fileName;
							client.DownloadFile(new Uri(apod.url), lowimg);
						}

						if (!string.IsNullOrEmpty(apod.hdurl))
						{
							string fileName = filePattern.Replace("##DATETIME##", (dt ?? DateTime.Now).ToString("yyyy-MM-dd"))
														 .Replace("##QUALITY##", "HD")
														 .Replace("##TITLE##", normalizeName(apod.title));
							hiresimg = @".\images\HD\" + fileName;
							client.DownloadFile(new Uri(apod.hdurl), hiresimg);
						}
						Console.WriteLine(string.Format("NASA have published  at {0}.\nPicture '{1}' => SRes = {2} and HiRes = {3}.\n\n", apod.date, apod.title, lowimg, hiresimg));
					}
					html += "<div class='nasaDate'>";
					html += apod.date;
					html += "</div>";
					html += "<div class='nasaTitle'>";
					html += apod.title;
					html += "</div>";
					html += "<div class='nasaExplanation'>";
					html += apod.explanation;
					html += "</div>";
					html += "<div class='nasaImage'>";
					if (htmlLoadImagesFromInternet)
					{
						html += string.Format("<img class='nasaPic' src='{0}' lowsrc='{1}' alt='{2}'>", apod.hdurl, apod.url, apod.title);
					}
					else
					{
						html += string.Format("<img class='nasaPic' src='{0}' lowsrc='{1}' alt='{2}'>", hiresimg, lowimg, apod.title);
					}
					html += "</div>";
					if (!string.IsNullOrEmpty(apod.copyright))
					{
						html += "<div class='nasaCopyright'>";
						html += "<span style='font-weight:bold;font-style=none;'>Copyright</span> - " + apod.copyright;
						html += "</div>";
					}
				}
				else if (apod.media_type == "video")
				{
					html += "<div class='nasaDate'>";
					html += apod.date;
					html += "</div>";
					html += "<div class='nasaTitle'>";
					html += apod.title;
					html += "</div>";
					html += "<div class='nasaExplanation'>";
					html += apod.explanation;
					html += "</div>";
					html += "<div class='nasaImage'>";
					html += string.Format("<embed class='nasaVideo' src='{0}' alt='{1}'>", apod.url, apod.title);
					html += "</div>";
					if (!string.IsNullOrEmpty(apod.copyright))
					{
						html += "<div class='nasaCopyright'>";
						html += "<span style='font-weight:bold;font-style=none;'>Copyright</span> - " + apod.copyright;
						html += "</div>";
					}
				}
				else
				{
					Console.WriteLine("Media Type '{0}' is not included to html.\n\n", apod.media_type);
				}
				Console.WriteLine(string.Format("NASA have published a {1} at {0}.\nBrowse '{3}' at {2}.\n\n", apod.date, apod.media_type, apod.url, apod.title));

				html += "  </div>";
			}
			else
			{
				Console.WriteLine(string.Format("NASA do not have published image at {0}.\n\n", (dt ?? DateTime.Now).ToString("yyyy-MM-dd")));
			}
			return html;
		}

		/// <summary>
		/// Normalize Title to fill filename correctly
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		static string normalizeName(string name)
		{
			return name.Replace(" ", "-").Replace("?", "").Replace("\\", "").Replace("'", "").ToUpper();
		}
	}
}
