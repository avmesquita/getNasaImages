using System;
using System.IO;
using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Net.NetworkInformation;

namespace GetNasaImages
{
	class Program
	{
		public static IConfigurationRoot _configuration;
		public static INasaService _nasaService;
		public static ILogger _logger;

		static string filePattern = "Nasa-APOD-##DATETIME##-##QUALITY##-##TITLE##.jpg";
		static string filePath = @".\images\";
		static string filePathSD = @".\images\SD";
		static string filePathHD = @".\images\HD";

		/// <summary>
		/// Main routine
		/// </summary>
		/// <param name="args"></param>
		static void Main(string[] args)
		{
			ServiceCollection serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);

			IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

			validateFolders();

			loopToPast((args.Length >= 1) ? IsInteger(args[0]) : 1);
		}

		/// <summary>
		/// .net core pattern
		/// </summary>
		/// <param name="serviceCollection"></param>
		private static void ConfigureServices(ServiceCollection serviceCollection)
		{
			_configuration = new ConfigurationBuilder()
								   .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
								   .AddJsonFile("appsettings.json", false)
								   .Build();

			serviceCollection.AddSingleton<IConfigurationRoot>(_configuration);

			serviceCollection.AddLogging(configure => configure.AddConsole().AddConfiguration(_configuration));

			ILoggerFactory loggerFactory = new LoggerFactory();
			_logger = loggerFactory.CreateLogger<Program>();			

			_nasaService = new NasaService(_configuration["NasaApiKey"]);

			serviceCollection.AddSingleton<INasaService>(_nasaService);

			_logger.LogInformation("[OK] Configure Services");			
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
			html.AppendLine("     <link href='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/css/bootstrap.min.css' rel='stylesheet' integrity='sha384-9aIt2nRpC12Uk9gS9baDl411NQApFmC26EwAOH8WgZl5MYYxFfc+NcPb1dKGj7Sk' crossorigin='anonymous'>");
			html.AppendLine("     <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.min.js' integrity='sha384-OgVRvuATP1z7JjHLkuOU7Xw704+h835Lr+6QL9UvYjZE3Ipu6Tp75j7Bh/kR0JKI' crossorigin='anonymous'></script>");
			html.AppendLine("     <script src='https://stackpath.bootstrapcdn.com/bootstrap/4.5.0/js/bootstrap.bundle.min.js' integrity='sha384-1CmrxMRARb6aLqgBO7yyAxTOQE2AKb9GfXnEo760AUcUmFx3ibVJJAzGytlQcNXd' crossorigin='anonymous'></script>");
			html.AppendLine("     <link href='https://stackpath.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css' rel='stylesheet'>");
			html.AppendLine("     <link href='https://stackpath.bootstrapcdn.com/bootswatch/4.5.0/superhero/bootstrap.min.css' rel='stylesheet'>");
			html.AppendLine("     <script src='https://stackpath.bootstrapcdn.com/bootlint/1.1.0/bootlint.min.js'></script>");
			html.AppendLine("    <STYLE>");
			html.AppendLine("      .nasaDay{ padding: 20px; } ");
			html.AppendLine("      .nasaDate{ font-weight:bold; } ");
			html.AppendLine("      .nasaTitle{ font-weight:bold; } ");
			html.AppendLine("      .nasaExplanation{ text-align: justify; text-justify: inter - word; padding-top: 10px; padding-bottom: 10px; } ");
			html.AppendLine("      .nasaCopyright{ font-style:italic; } ");
			html.AppendLine("      .nasaImage{} ");
			html.AppendLine("      .nasaPic{ width:100% } ");
			html.AppendLine("      .nasaVideo{} ");
			html.AppendLine("    </STYLE>");

			html.AppendLine("  </HEAD>");
			html.AppendLine("  <BODY>");

			while (count != days)
			{
				NasaAPOD apod = _nasaService.getAPOD(false, dt);
				string htmlSection = SaveAPOD(apod, dt);

				html.AppendLine(htmlSection);

				dt = dt?.AddDays(-1);
				count++;
			}
			html.AppendLine("  </BODY>");
			html.AppendLine("</HTML>");

			System.IO.File.WriteAllText(@".\NasaAPOD.html", html.ToString());
			Console.WriteLine(@"Webpage .\NasaAPOD.html generated with all range images.");
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
				try
				{
					html += "  <div class='nasaDay'>";

					if (apod.media_type == "image")
					{
						string lowimg = string.Empty;
						string hiresimg = string.Empty;

						using (WebClient client = new WebClient())
						{
							if (!string.IsNullOrEmpty(apod.url))
							{
								lowimg = getFileName(dt, "SD", apod.title);

								if (!File.Exists(lowimg))
									client.DownloadFile(new Uri(apod.url), lowimg);
								else
									Console.WriteLine("File already exists. Download skipped.");
							}

							if (!string.IsNullOrEmpty(apod.hdurl))
							{
								hiresimg = getFileName(dt, "HD", apod.title);
								if (!File.Exists(hiresimg))
									client.DownloadFile(new Uri(apod.hdurl), hiresimg);
								else
									Console.WriteLine("File already exists. Download skipped.");
							}
							Console.WriteLine(string.Format("NASA have published a picture at {0}.\nBrowse '{1}' => \nSRes  = {2} \nHiRes = {3}.\n\n", apod.date, apod.title, lowimg, hiresimg));
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
						Console.WriteLine(string.Format("NASA have published a video at {0}.\nBrowse '{3}' => \n{2}.\n\n", apod.date, apod.media_type, apod.url, apod.title));
					}
					else
					{
						if (!string.IsNullOrEmpty(apod.media_type))
						{
							Console.WriteLine("Media Type '{0}' is not included to html.\n\n", apod.media_type);
							Console.WriteLine(string.Format("NASA have published a {1} at {0}.\nBrowse '{3}' => \n{2}.\n\n", apod.date, apod.media_type, apod.url, apod.title));
						}
					}

					html += "  </div>";
				}
				catch (Exception ex) 
				{ 
					Console.WriteLine("Error downloading {0} \n\n{1}\n\n", apod.hdurl,ex.Message);
					_logger.LogError(ex, ex.Message);
				}
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
			return name.Replace(" ", "-")
					   .Replace("?", "-")
					   .Replace(@"\", "-")
					   .Replace("/", "-")
					   .Replace("+", "-")
					   .Replace("%", "-")
					   .Replace("!", "-")
					   .Replace("'", "")
					   .Replace(",", "-")
					   .Replace(";", "-")
					   .Replace(":", "-")
					   .ToUpper();
		}

		static string getFileName(DateTime? dt, string quality, string title)
		{
			return (quality == "SD" ? filePathSD : filePathHD) 
				   + filePattern.Replace("##DATETIME##", (dt ?? DateTime.Now).ToString("yyyy-MM-dd"))
					            .Replace("##QUALITY##", quality)
					            .Replace("##TITLE##", normalizeName(title));
		}

		static void validateFolders()
		{
			if (!Directory.Exists(filePath))
				Directory.CreateDirectory(filePath);

			if (!Directory.Exists(filePathSD))
				Directory.CreateDirectory(filePathSD);

			if (!Directory.Exists(filePathHD))
				Directory.CreateDirectory(filePathHD);
		}
	}
}
