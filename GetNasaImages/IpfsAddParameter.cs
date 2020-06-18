using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace GetNasaImages
{
	public class IpfsAddParameter
	{
		/// <summary>
		/// Write minimal output.
		/// Required: no.
		/// </summary>
		public bool quiet { get; set; }

		/// <summary>
		/// Write only final hash.
		/// Required: no.
		/// </summary>
		public bool quieter { get; set; }

		/// <summary>
		/// Write no output.
		/// Required: no.
		/// </summary>
		public bool silent { get; set; }

		/// <summary>
		/// Stream progress data.
		/// Required: no.
		/// </summary>
		public bool progress { get; set; }

		/// <summary>
		/// Use trickle-dag format for dag generation.
		/// Required: no.
		/// </summary>
		public bool trickle { get; set; }

		/// <summary>
		/// Only chunk and hash - do not write to disk. 
		/// Required: no.
		/// </summary>

		[Display(Name = "only-hash")]
		[JsonProperty("only-hash")]
		public bool onlyhash { get; set; }

		/// <summary>
		/// Wrap files with a directory object. 
		/// Required: no.
		/// </summary>		
		[Display(Name = "wrap-with-directory")]
		[JsonProperty("wrap-with-directory")]
		public bool wrapwithdirectory { get; set; }

		/// <summary>
		/// Chunking algorithm, size-[bytes], rabin-[min]-[avg]-[max] or buzhash. 
		/// Default: size-262144. 
		/// Required: no.
		/// </summary>
		public string chunker { get; set; } = "size-262144";

		/// <summary>
		/// Pin this object when adding.
		/// Default: true. 
		/// Required: no.
		/// </summary>
		public bool pin { get; set; } = true;

		/// <summary>
		/// Use raw blocks for leaf nodes. (experimental). 
		/// Required: no.
		/// </summary>		
		[Display(Name = "raw-leaves")]
		[JsonProperty("raw-leaves")]
		public bool rawleaves { get; set; }

		/// <summary>
		/// Add the file using filestore. Implies raw-leaves. (experimental). 
		/// Required: no.
		/// </summary>
		public bool nocopy { get; set; }

		/// <summary>
		/// Check the filestore for pre-existing blocks. (experimental). 
		/// Required: no.
		/// </summary>
		public bool fscache { get; set; }

		/// <summary>
		/// CID version.
		/// Defaults to 0 unless an option that depends on CIDv1 is passed. (experimental). 
		/// Required: no.
		/// </summary>
		[Display(Name = "cid-version")]
		[JsonProperty("cid-version")]
		public int cidversion { get; set; } = 0;

		/// <summary>
		/// Hash function to use. Implies CIDv1 if not sha2-256. (experimental). 
		/// Default: sha2-256. 
		/// Required: no.
		/// </summary>
		public string hash { get; set; } = "sha2-256";

		/// <summary>
		/// Inline small blocks into CIDs. (experimental). 
		/// Required: no.
		/// </summary>
		public bool inline { get; set; }

		/// <summary>
		/// Maximum block size to inline. (experimental). 
		/// Default: 32. 
		/// Required: no.
		/// </summary>
		[Display(Name = "inline-limit")]
		[JsonProperty("inline-limit")]
		public int inlinelimit { get; set; } = 32;
	}
}
