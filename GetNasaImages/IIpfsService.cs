using System;
using System.Collections.Generic;
using System.Text;

namespace GetNasaImages
{
	public interface IIpfsService
	{
		void ListPeers();
		IpfsAddResponse PostFile(string url);
		IpfsAddResponse PostLocalFile(string path);
	}
}
