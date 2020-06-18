using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GetNasaImages
{
	public class IpfsService : IIpfsService
	{
		private const string peerAddress = "/api/v0/swarm/peers";
		private const string addFile = "/api/v0/add/";
		private string host { get; set; }
		public IList<IpfsPeer> Peers { get; internal set; }

		public IpfsService(string host)
		{
			if (!string.IsNullOrEmpty(host))
			{
				this.host = host;
				ListPeers();
			}
		}

		public void ListPeers()
		{
			try
			{
				this.Peers = new List<IpfsPeer>();

				using (var client = new HttpClient(new System.Net.Http.HttpClientHandler()))
				{
					var result = client.PostAsync(host + peerAddress, null);

					var retobj = result.Result.Content.ReadAsStringAsync();

					if (result.Result.StatusCode == System.Net.HttpStatusCode.OK)
					{
						var peers = JsonConvert.DeserializeObject<IpfsPeers>(retobj.Result);
						this.Peers = peers.Peers;
					}
				}
			}
			catch (Exception ex)
			{ }
		}

		public IpfsAddResponse PostFile(string url)
		{
			try
			{
				using (var client = new HttpClient(new System.Net.Http.HttpClientHandler()))
				{
					var image = getImageFromURL(url);

					MemoryStream ms = new MemoryStream(image);
					var content = new MultipartFormDataContent();
					var streamContent = new StreamContent(ms);
					streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					content.Add(streamContent, "file", System.IO.Path.GetFileName(url));

					var response = client.PostAsync(host + addFile, content);

					var retobj = response.Result.Content.ReadAsStringAsync();

					var deserialized = JsonConvert.DeserializeObject<IpfsAddResponse>(retobj.Result.ToString());

					return deserialized;
				}
			}
			catch
			{
				return null;
			}
		}

		public IpfsAddResponse PostLocalFile(string path)
		{
			try
			{
				using (var client = new HttpClient(new System.Net.Http.HttpClientHandler()))
				{

					var htmlFile = System.IO.File.ReadAllBytes(path);
					MemoryStream ms = new MemoryStream(htmlFile);
					var content = new MultipartFormDataContent();
					var streamContent = new StreamContent(ms);
					streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					content.Add(streamContent, "file", System.IO.Path.GetFileName(path));

					var response = client.PostAsync(host + addFile, content);

					var retobj = response.Result.Content.ReadAsStringAsync();

					var deserialized = JsonConvert.DeserializeObject<IpfsAddResponse>(retobj.Result.ToString());

					return deserialized;
				}
			}
			catch
			{
				return null;
			}
		}

		private byte[] getImageFromURL(string url)
		{
			var webClient = new System.Net.WebClient();
			byte[] imageBytes = webClient.DownloadData(url);

			return imageBytes;
		}

	}
}
