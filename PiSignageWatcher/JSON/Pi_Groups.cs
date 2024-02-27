using System;
using System.Collections.Generic;

namespace PiSignageWatcher.JSON
{
	internal class Pi_Groups
	{
		// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
		public class Ads
		{
			public bool adPlaylist { get; set; }
			public int adCount { get; set; }
			public int adInterval { get; set; }
		}

		public class Audio
		{
			public bool enable { get; set; }
			public bool random { get; set; }
			public int volume { get; set; }
		}

		public class CreatedBy
		{
			public string _id { get; set; }
			public string name { get; set; }
		}

		public class Datum
		{
			public Sleep sleep { get; set; }
			public Reboot reboot { get; set; }
			public KioskUi kioskUi { get; set; }
			public ShowClock showClock { get; set; }
			public MonitorArrangement monitorArrangement { get; set; }
			public EmergencyMessage emergencyMessage { get; set; }
			public CreatedBy createdBy { get; set; }
			public bool disableHwWidgets { get; set; }
			public bool enablePio { get; set; }
			public string _id { get; set; }
			public List<Playlist> playlists { get; set; }
			public bool combineDefaultPlaylist { get; set; }
			public bool playAllEligiblePlaylists { get; set; }
			public bool shuffleContent { get; set; }
			public bool alternateContent { get; set; }
			public int timeToStopVideo { get; set; }
			public List<string> assets { get; set; }
			public List<object> assetsValidity { get; set; }
			public List<DeployedPlaylist> deployedPlaylists { get; set; }
			public List<string> deployedAssets { get; set; }
			public List<object> labels { get; set; }
			public bool deployEveryday { get; set; }
			public bool enableMpv { get; set; }
			public string mpvAudioDelay { get; set; }
			public string selectedVideoPlayer { get; set; }
			public bool disableWebUi { get; set; }
			public bool disableWarnings { get; set; }
			public bool disableAp { get; set; }
			public string installation { get; set; }
			public string orientation { get; set; }
			public bool animationEnable { get; set; }
			public object animationType { get; set; }
			public bool resizeAssets { get; set; }
			public bool videoKeepAspect { get; set; }
			public bool videoShowSubtitles { get; set; }
			public bool imageLetterboxed { get; set; }
			public string signageBackgroundColor { get; set; }
			public bool urlReloadDisable { get; set; }
			public bool keepWeblinksInMemory { get; set; }
			public bool loadPlaylistOnCompletion { get; set; }
			public string resolution { get; set; }
			public int omxVolume { get; set; }
			public object logo { get; set; }
			public int logox { get; set; }
			public int logoy { get; set; }
			public string name { get; set; }
			public DateTime createdAt { get; set; }
			public int __v { get; set; }
			public string playlistToSchedule { get; set; }
			public object deployedTicker { get; set; }
			public string lastDeployed { get; set; }
		}

		public class DeployedPlaylist
		{
			public string name { get; set; }
			public Settings settings { get; set; }
			public bool skipForSchedule { get; set; }
			public string plType { get; set; }
		}

		public class EmergencyMessage
		{
			public string msg { get; set; }
			public string hPos { get; set; }
			public string vPos { get; set; }
		}

		public class KioskUi
		{
			public bool enable { get; set; }
		}

		public class MonitorArrangement
		{
			public string mode { get; set; }
			public bool reverse { get; set; }
		}

		public class Playlist
		{
			public string name { get; set; }
			public Settings settings { get; set; }
			public bool skipForSchedule { get; set; }
			public string plType { get; set; }
		}

		public class Reboot
		{
			public bool enable { get; set; }
		}

		public class Root
		{
			public string stat_message { get; set; }
			public List<Datum> data { get; set; }
			public bool success { get; set; }
		}

		public class Settings
		{
			public Ads ads { get; set; }
			public Audio audio { get; set; }
		}

		public class ShowClock
		{
			public bool enable { get; set; }
			public string format { get; set; }
			public string position { get; set; }
		}

		public class Sleep
		{
			public bool enable { get; set; }
		}


	}
}
