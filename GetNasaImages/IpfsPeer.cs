using System;
using System.Collections.Generic;
using System.Text;

namespace GetNasaImages
{
	public class IpfsPeer
	{
		public string Addr { get; set; }
		public string Peer { get; set; }
		public string Latency { get; set; }
		
		public string Muxer { get; set; }
		
		public string Direction { get; set; }
		
		public string Streams { get; set; }
	}
}
