using System.Collections.Generic;

namespace PiSignageWatcher.JSON
{
	public class Pi_Playlists
	{
		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
		public class Asset
		{
			public string filename { get; set; }
			public int duration { get; set; }
			public bool selected { get; set; }
			public Option option { get; set; }
			public bool fullscreen { get; set; }
			public bool expired { get; set; }
			public bool deleted { get; set; }
			public string side { get; set; }
			public string bottom { get; set; }
			public string zone4 { get; set; }
			public string zone5 { get; set; }
			public bool? isVideo { get; set; }
		}

		public class Datum
		{
			public List<Asset> assets { get; set; }
			public string name { get; set; }
			public string layout { get; set; }
			public string templateName { get; set; }
			public object videoWindow { get; set; }
			public ZoneVideoWindow zoneVideoWindow { get; set; }
			public Schedule schedule { get; set; }
			public List<object> labels { get; set; }
			public object groupIds { get; set; }
			public List<string> belongsTo { get; set; }
			public Settings settings { get; set; }
		}

		public class Option
		{
			public bool main { get; set; }
		}

		public class Root
		{
			public string stat_message { get; set; }
			public List<Datum> data { get; set; }
			public bool success { get; set; }
		}

		public class Schedule
		{
		}

		public class Settings
		{
		}

		public class ZoneVideoWindow
		{
		}


	}
}
