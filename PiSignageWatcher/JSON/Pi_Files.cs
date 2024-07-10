using System;
using System.Collections.Generic;

namespace PiSignageWatcher.JSON
{
	internal class Pi_Files
	{
		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
		public class CreatedBy
		{
			public string Id { get; set; }
			public string Name { get; set; }
		}

		public class Data
		{
			public Sizes Sizes { get; set; }
			public List<string> Files { get; set; }
			public List<Dbdatum> Dbdata { get; set; }
			public List<string> SystemAssets { get; set; }
			public bool Player { get; set; }
		}

		public class Dbdatum
		{
			public CreatedBy CreatedBy { get; set; }
			public List<object> Labels { get; set; }
			public string Id { get; set; }
			public string Name { get; set; }
			public string Type { get; set; }
			public string Duration { get; set; }
			public string Size { get; set; }
			public List<string> Playlists { get; set; }
			public List<object> GroupIds { get; set; }
			public string Installation { get; set; }
			public DateTime CreatedAt { get; set; }
			public int V { get; set; }
			public Resolution Resolution { get; set; }
			public string Thumbnail { get; set; }
		}

		public class Resolution
		{
			public string Width { get; set; }
			public string Height { get; set; }
		}

		public class Root
		{
			public string StatMessage { get; set; }
			public Data Data { get; set; }
			public bool Success { get; set; }
		}

		public class Sizes
		{
			public int Total { get; set; }
			public int Used { get; set; }
		}
	}
}
