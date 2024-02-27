using System;
using System.Collections.Generic;

namespace PiSignageWatcher
{
	public class Resolution
	{
		public string width { get; set; }
		public string height { get; set; }
	}

	public class CreatedBy
	{
		public string _id { get; set; }
		public string name { get; set; }
	}

	public class Dbdata
	{
		public Resolution resolution { get; set; }
		public CreatedBy createdBy { get; set; }
		public List<object> labels { get; set; }
		public List<object> playlists { get; set; }
		public List<object> groupIds { get; set; }
		public string installation { get; set; }
		public string _id { get; set; }
		public DateTime createdAt { get; set; }
		public string name { get; set; }
		public string type { get; set; }
		public string duration { get; set; }
		public string size { get; set; }
		public string thumbnail { get; set; }
		public int __v { get; set; }
	}

	public class Data_Files
	{
		public string name { get; set; }
		public string size { get; set; }
		public DateTime ctime { get; set; }
		public string path { get; set; }
		public string type { get; set; }
		public Dbdata dbdata { get; set; }
	}

	public class Root_Files
	{
		public string stat_message { get; set; }
		public Data_Files data { get; set; }
		public bool success { get; set; }
	}

	public class ZoneAliases
	{
		public string side { get; set; }
		public string bottom { get; set; }
		public string zone4 { get; set; }
		public string zone5 { get; set; }
		public string zone6 { get; set; }
	}

	public class UiDefaults
	{
		public string playlistView { get; set; }
	}

	public class Settings
	{
		public ZoneAliases zoneAliases { get; set; }
		public UiDefaults uiDefaults { get; set; }
		public bool signupForBeta { get; set; }
		public bool serveOldUi { get; set; }
		public bool newLayoutsEnable { get; set; }
		public bool systemMessagesHide { get; set; }
		public bool forceTvOn { get; set; }
		public bool disableCECPowerCheck { get; set; }
		public bool hideWelcomeNotice { get; set; }
		public int defaultDuration { get; set; }
		public string language { get; set; }
		public object sshPassword { get; set; }
		public bool enableLog { get; set; }
		public bool subscribeForAlerts { get; set; }
		public int reportIntervalMinutes { get; set; }
		public bool enableYoutubeDl { get; set; }
		public bool licenseOnly { get; set; }
		public bool shareableLabels { get; set; }
		public bool playerAutoRegistration { get; set; }
		public bool disableDownload { get; set; }
		public bool disablePlayerDownloadOnModem { get; set; }
	}

	public class UserInfo
	{
		public string username { get; set; }
		public string email { get; set; }
		public string role { get; set; }
		public string provider { get; set; }
		public Settings settings { get; set; }
		public string _id { get; set; }
	}

	public class Root_Session
	{
		public UserInfo userInfo { get; set; }
		public string token { get; set; }
	}

	public class Datum_Files_Upload
	{
		public string name { get; set; }
		public int size { get; set; }
		public string type { get; set; }
	}

	public class Root_Files_Upload
	{
		public string stat_message { get; set; }
		public List<Datum_Files_Upload> data { get; set; }
		public bool success { get; set; }
	}

	public class Asset_Files
	{
		public string filename { get; set; }
		public int duration { get; set; }
		public bool selected { set; get; }
		public bool isVideo { set; get; }
		public bool dragSelected { set; get; }
		public bool fullscreen { get; set; }
		public string side { get; set; }
		public string bottom { get; set; }
		public string zone4 { get; set; }
		public string zone5 { get; set; }
	}

	public class Group
	{
		public string name { get; set; }
		public string _id { get; set; }
	}

	public class Object
	{
		public Group group { get; set; }
		public CreatedBy createdBy { get; set; }
		public object lastUpload { get; set; }
		public bool newSocketIo { get; set; }
		public bool webSocket { get; set; }
		public bool registered { get; set; }
		public bool serverServiceDisabled { get; set; }
		public List<object> labels { get; set; }
		public string installation { get; set; }
		public bool licensed { get; set; }
		public bool cecTvStatus { get; set; }
		public bool disabled { get; set; }
		public string _id { get; set; }
		public string version { get; set; }
		public string platform_version { get; set; }
		public string cpuSerialNumber { get; set; }
		public string myIpAddress { get; set; }
		public string ethMac { get; set; }
		public string wifiMac { get; set; }
		public string ip { get; set; }
		public bool playlistOn { get; set; }
		public string currentPlaylist { get; set; }
		public object playlistStarttime { get; set; }
		public string diskSpaceUsed { get; set; }
		public string diskSpaceAvailable { get; set; }
		public string duration { get; set; }
		public bool tvStatus { get; set; }
		public DateTime lastReported { get; set; }
		public string socket { get; set; }
		public DateTime createdAt { get; set; }
		public bool isConnected { get; set; }
		public int __v { get; set; }
		public string TZ { get; set; }
		public string name { get; set; }
		public bool syncInProgress { get; set; }
		public string wgetSpeed { get; set; }
		public string uptime { get; set; }
		public string piTemperature { get; set; }
		public string wgetBytes { get; set; }
	}

	public class CurrentVersion
	{
		public string version { get; set; }
		public string versionP2 { get; set; }
	}

	public class Data_Player
	{
		public List<Object> objects { get; set; }
		public int page { get; set; }
		public int pages { get; set; }
		public int count { get; set; }
		public CurrentVersion currentVersion { get; set; }
	}

	public class Root_Player
	{
		public string stat_message { get; set; }
		public Data_Player data { get; set; }
		public bool success { get; set; }
	}
}
