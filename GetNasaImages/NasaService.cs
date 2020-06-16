using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Configuration;
using System.Net;

namespace GetNasaImages
{
	public class NasaService : INasaService
	{
		string api_key { get; set; }

		private string apodEndpoint = "https://api.nasa.gov/planetary/apod";

		public NasaService(string apikey)
		{
			this.api_key = apikey;
		}

		#region [ Astronomic Pic of the Day ]

		public NasaAPOD getAPOD(bool isHD = false, DateTime? date = null)
		{
			string endpoint = string.Empty;

			if (date == null)
			{
				endpoint = $"{this.apodEndpoint}?api_key={this.api_key}&hd={isHD.ToString().ToLower()}";
			}
			else
			{
				var data = Convert.ToDateTime(date);

				endpoint = $"{this.apodEndpoint}?api_key={this.api_key}&hd={isHD.ToString().ToLower()}&date={data.ToString("yyyy-MM-dd")}";
			}

			using (var client = new HttpClient(new System.Net.Http.HttpClientHandler()))
			{
				var result = client.GetAsync(endpoint);

				var retobj = result.Result.Content.ReadAsStringAsync();

				var deserialized = JsonConvert.DeserializeObject<NasaAPOD>(retobj.Result.ToString());

				return deserialized;
			}
		}

		public byte[] getAstronomicPicOfDay(bool isHD = false, DateTime? date = null)
		{
			var apod = this.getAPOD(isHD, date);

			if (isHD)
			{
				if (string.IsNullOrEmpty(apod.hdurl))
					return this.getImageFromURL(apod.url);
				else
					return this.getImageFromURL(apod.hdurl);
			}
			else
			{
				if (string.IsNullOrEmpty(apod.url))
					return this.getImageFromURL(apod.hdurl);
				else
					return this.getImageFromURL(apod.url);
			}
		}

		private byte[] getImageFromURL(string url)
		{
			try
			{
				using (var client = new HttpClient(new System.Net.Http.HttpClientHandler()))
				{
					var result = client.GetByteArrayAsync(url);

					return result.Result;
				}
			}
			catch
			{
				return null;
			}
		}

		#endregion

	}
}
