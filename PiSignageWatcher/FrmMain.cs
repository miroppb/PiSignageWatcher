using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RestSharp;
using miroppb;
using System.Threading.Tasks;
using System.Net.Http;
using Dapper;

namespace PiSignageWatcher
{
    public partial class FrmMain : Form
    {
        private string APIUrl = null;
        private string gDrive = null;
        private string Prowl = null;
        protected string token = "";
        private List<ClGroups> groups = new List<ClGroups>();
        private List<ClPlaylists> playlists = new List<ClPlaylists>();
        private List<ClSchedule> Action = new List<ClSchedule>();
        List<ClTV> tvs = new List<ClTV>();

        const string announcements_id = Config.announcements_id;

        public FrmMain()
        {
            InitializeComponent();

            libmiroppb.Log("Welcome to PiSignage Watcher! ~uWu~");
            PopulateData();

            timerRefresh_Tick(null, null);
        }

        private void PopulateData()
        {
            //settings
            using (SQLiteConnection conn = GetSQLConnection())
            {
                ClSettings settings = conn.Query<ClSettings>("SELECT api, gdrive, prowl FROM settings").FirstOrDefault();
                APIUrl = settings.api;
                gDrive = settings.gdrive;
                Prowl = settings.prowl;
            }

            //groups
            (contextMenuStrip1.Items[2] as ToolStripMenuItem).DropDownItems.Clear();
            (contextMenuStrip1.Items[4] as ToolStripMenuItem).DropDownItems.Clear();
            using (SQLiteConnection conn = GetSQLConnection())
            {
                List<ClGroups> _groups = conn.Query<ClGroups>("SELECT * FROM groups").ToList();
                foreach (ClGroups group in _groups)
                {
                    groups.Add(group);
                    (contextMenuStrip1.Items[2] as ToolStripMenuItem).DropDownItems.Add(group.name, null, reDeployGroup);
                    (contextMenuStrip1.Items[4] as ToolStripMenuItem).DropDownItems.Add(group.name, null, rebootGroup);
                }
            }

            //playlists
            using (SQLiteConnection conn = GetSQLConnection())
            {
                List<ClPlaylists> _playlists = conn.Query<ClPlaylists>("SELECT * FROM playlists").ToList();
                foreach (ClPlaylists playlist in _playlists)
                    playlists.Add(playlist);
            }

            //tvs
            using (SQLiteConnection conn = GetSQLConnection())
            {
                List<ClTV> _tvs = conn.Query<ClTV>("SELECT * FROM tvs").ToList();
                foreach (ClTV tv in _tvs)
                    tvs.Add(tv);
            }

            //read schedules into dictionary
            using (SQLiteConnection conn = GetSQLConnection())
            {
                List<ClSchedule> _schedule = conn.Query<ClSchedule, ClTV, ClSchedule>
                    ("SELECT s.day, s.time, s.action, tv.* FROM schedule AS s INNER JOIN tvs AS tv ON tv.name = s.tv", (s, t) =>
                    {
                        s.tv = t;
                        return s;
                    }, splitOn: "name").ToList();
                libmiroppb.Log("Using following schedule:");
                foreach (ClSchedule schedule in _schedule)
                {
                    Action.Add(schedule);
                    libmiroppb.Log($"[{schedule.tv.name}, {schedule.day.ToString()}, {schedule.time.ToShortTimeString()}, {schedule.action.ToString()}]");
                }
            }
            timerSchedule.Enabled = true;
            timerSchedule.Start();
            libmiroppb.Log("Schedule Timer started");
        }

        private async Task<bool> refreshToken()
        {
            ClSettings _settings = null;
            using (SQLiteConnection conn = GetSQLConnection())
            {
                _settings = conn.Query<ClSettings>("SELECT user, pass FROM settings").ToList().FirstOrDefault();
            }

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "email", _settings.user },
                { "password", _settings.pass },
                { "getToken", "true" }
            };

            string json = SendRequest("/session", Method.Post, data, false);

            Root_Session t = JsonConvert.DeserializeObject<Root_Session>(json);
            while (t == null)
            {
                libmiroppb.Log("Token not provided, retrying after 1 minute...");
                //retrying after a few minutes... //2.24.22
                await Task.Delay(60000);
                json = SendRequest("/session", Method.Post, data, false);
                t = JsonConvert.DeserializeObject<Root_Session>(json);
            }
            token = t.token;
            libmiroppb.Log("Refreshed token: " + token);
            return true;
        }

        private async void timerRefresh_Tick(object sender, EventArgs e)
        {
            //lets get authenticated
            if (await refreshToken())
            {
                bool changes = false;

                //get list of files
                libmiroppb.Log("Getting list of Google Drive files...");
                Dictionary<string, string> gd_files = GetGoogleDriveFiles();
                if (gd_files == null) { return; }
                else { foreach (string file in gd_files.Keys) { libmiroppb.Log(file); } } //print each GD file to log

                List<ClFiles> _files = null;
                using (SQLiteConnection conn = GetSQLConnection())
                {
                    libmiroppb.Log("Getting list of database files...");
                    _files = conn.Query<ClFiles>("SELECT * FROM files").ToList();
                }

                List<string> db_files = new List<string>();
                libmiroppb.Log("DB files:");
                foreach (ClFiles file in _files) { db_files.Add(file.filename); libmiroppb.Log(file.filename); }

                //compare the db files with gd files
                foreach (KeyValuePair<string, string> file in gd_files)
                {
                    //for each file that doesn't exist already (new file) and starts with a playlist "search" term
                    if (!db_files.Contains(file.Key) && playlists.Any(x => x.search.Contains(file.Key.Split(' ')[0])))
                    {
                        libmiroppb.Log("Working on: " + file.Key);
                        _ = SendNotificationAsync("Working on " + file.Key);

                        //if gd file exists but db doesnt, we need to download
                        GoogleDownloadFile(file.Value, file.Key);
                        libmiroppb.Log("Downloaded: " + file.Key);

                        File.Move(file.Key, file.Key.Replace(",", ""));

                        //and then upload to pisignage
                        string up = "";
                        while (up == "") //1.20.22 In case uploading fails (for unknown reason)
                        {
                            up = Upload("/files", file.Key);
                            libmiroppb.Log("Upload attempt: " + file.Key + ", Response: " + up);
                            _ = SendNotificationAsync("Upload attempt " + file.Key);
                        }
                        libmiroppb.Log("Uploaded: " + file.Key + ", Response: " + up);
                        _ = SendNotificationAsync("Uploaded " + file.Key);

                        File.Delete(file.Key);
                        libmiroppb.Log("Deleted local file: " + file.Key);

                        Root_Files_Upload rfu = JsonConvert.DeserializeObject<Root_Files_Upload>(up);
                        //process upload
                        string post = SendRequest("/postupload", Method.Post, new { files = rfu.data });
                        libmiroppb.Log("PostUpload: " + file.Key + ", Response: " + post);
                        await Task.Delay(1000);

                        //we'll have only 1 file per TV, for now
                        string json = SendRequest("/files/" + file.Key, Method.Get, null);
                        libmiroppb.Log("Getting: " + file.Key + ", Response: " + json);
                        Root_Files rf = JsonConvert.DeserializeObject<Root_Files>(json);

                        //add current file to playlist
                        Asset_Files af = new()
                        {
                            filename = file.Key,
                            dragSelected = false,
                            fullscreen = true,
                            isVideo = true,
                            selected = true,
                            duration = Convert.ToInt32(rf.data.dbdata.duration)
                        };
                        object[] resArray = new object[] { af };
                        string pl = playlists.First(x => x.search == file.Key.Split(' ')[0]).name; //playlist associated with current file
                        string playlistResponse = SendRequest("/playlists/" + pl, Method.Post, new { assets = resArray });
                        libmiroppb.Log("Added to playlist " + pl + ": " + file.Key + ", Response: " + playlistResponse);

                        //add file to database
                        using (SQLiteConnection conn = GetSQLConnection())
                        {
                            conn.Execute($"INSERT INTO files VALUES('{file.Key}', '{pl}');");
                        }
                        libmiroppb.Log("Added " + file.Key + " to the database");

                        changes = true;
                    }
                }
                foreach (string a in db_files)
                {
                    //else if db file exists but gd doesn't
                    if (!gd_files.ContainsKey(a))
                    {
                        //remove db file from pisignage
                        string files = SendRequest("/files/" + a, Method.Delete, null);
                        libmiroppb.Log("Deleted file: " + a + ", Response: " + files);
                        changes = true;

                        //and database
                        using (SQLiteConnection conn = GetSQLConnection())
                        {
                            conn.Execute($"DELETE FROM files WHERE filename = '{a}'");
                        }
                        libmiroppb.Log("Deleted file " + a + " from database");
                    }
                }
                //Deploy each group if something was changed
                if (changes)
                {
                    foreach (ClGroups group in groups)
                    {
                        libmiroppb.Log("Waiting 30 seconds...");
                        await Task.Delay(30000); //1.30.22 Waiting 30 seconds
                        using (SQLiteConnection conn = GetSQLConnection())
                        {
                            ClFiles files = conn.Query<ClFiles>($"SELECT filename FROM files WHERE playlist = '{group.name}'").FirstOrDefault();
                            ClDeployOptions deployOptions = new ClDeployOptions()
                            {
                                assets = new string[]
                                {
                                files.filename,
                                "__" + groups.Where(x => x.name == group.name).First().name + ".json",
                                "custom_layout.html"
                                }
                            };
                            string groupResponse = SendRequest("/groups/" + group.hex, Method.Post, deployOptions);
                            libmiroppb.Log("Deployed " + group.name + ", Response: " + groupResponse);
                        }
                    }
                }
            }
        }

        private Dictionary<string, string> GetGoogleDriveFiles()
        {
            string[] Scopes = { DriveService.Scope.Drive , DriveService.Scope.DriveReadonly };
            string ApplicationName = "PiSignage Watcher";
            Dictionary<string, string> gd_files = new Dictionary<string, string>();

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";

                //Using this method, to timeout if user doesn't respond to Google Authorization fast enough
                UserCredential cred = null;
                var thread = new Thread(() =>
                    cred = (GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        Scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore(credPath)).Result)
                    )
                { IsBackground = true };
                thread.Start();
                if (!thread.Join(120000))
                {
                    libmiroppb.Log("Timed-out. User didn't accept Google Authentication in time...");
                    _ = SendNotificationAsync("Google Authentication needed...");
                    return null;
                }
                else
                    credential = cred;

                /*credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;*/
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            //FilesResource.ListRequest listRequest = service.
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = "'" + announcements_id + "' in parents";

            // List files.
            try
            {
                IList<Google.Apis.Drive.v3.Data.File> files = listRequest.Execute()
               .Files;
                Console.WriteLine("Files:");
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        gd_files.Add(file.Name, file.Id);
                    }
                }
                else
                {
                    Console.WriteLine("No files found.");
                }
            }
            catch { libmiroppb.Log("GD Didn't return any files..."); return null; }
            return gd_files;
        }

        private void GoogleDownloadFile(string id, string fname)
        {
            string[] Scopes = { DriveService.Scope.Drive, DriveService.Scope.DriveReadonly };
            string ApplicationName = "PiSignage Watcher";
            Dictionary<string, string> gd_files = new Dictionary<string, string>();

            UserCredential credential;

            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            var request = service.Files.Get(id);
            var mstream = new System.IO.MemoryStream();

            // Add a handler which will be notified on progress changes.
            // It will notify on each chunk download and when the
            // download is completed or failed.
            request.MediaDownloader.ProgressChanged += (Google.Apis.Download.IDownloadProgress progress) =>
            {
                switch (progress.Status)
                {
                    case Google.Apis.Download.DownloadStatus.Downloading:
                        {
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Completed:
                        {
                            Console.WriteLine("Download complete.");
                            SaveStream(mstream, fname);
                            break;
                        }
                    case Google.Apis.Download.DownloadStatus.Failed:
                        {
                            Console.WriteLine("Download failed.");
                            break;
                        }
                }
            };
            request.Download(mstream);
        }

        private static void SaveStream(System.IO.MemoryStream stream, string saveTo)
        {
            using (FileStream file = new(saveTo, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }

        private string Upload(string url, string filename)
        {
            RestClient restClient = new(APIUrl);
            RestRequest restRequest = new(url + "?token=" + token);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.Method = Method.Post;
            restRequest.AddHeader("Content-Type", "multipart/form-data");
            restRequest.AddFile("content", filename);
            var response = restClient.Execute(restRequest);
            return response.Content;
        }

        private string SendRequest(string url, RestSharp.Method method, object json, bool sendtoken = true)
        {
            RestClient restClient = new(APIUrl);
            RestRequest restRequest = null;
            if (sendtoken)
                restRequest = new RestRequest(url + "?token=" + token);
            else
                restRequest = new RestRequest(url);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.Method = method;
            if (json != null)
                restRequest.AddJsonBody(json);
            var response = restClient.Execute(restRequest);
            return response.Content;
        }

        #region JSON
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

        public class Rss
        {
            public bool enable { get; set; }
            public object link { get; set; }
            public int feedDelay { get; set; }
        }

        public class Ticker
        {
            public bool enable { get; set; }
            public string behavior { get; set; }
            public int textSpeed { get; set; }
            public Rss rss { get; set; }
        }

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

        public class Settings_Playlists
        {
            public Ticker ticker { get; set; }
            public Ads ads { get; set; }
            public Audio audio { get; set; }
        }

        public class Asset
        {
            public string filename { get; set; }
            public int duration { get; set; }
            public bool fullscreen { get; set; }
            public bool isVideo { get; set; }
            public bool selected { get; set; }
            public bool deleted { get; set; }
        }

        public class ZoneVideoWindow
        {
        }

        public class Schedule
        {
        }

        public class Data_Playlists
        {
            public Settings_Playlists settings { get; set; }
            public string layout { get; set; }
            public List<Asset> assets { get; set; }
            public object videoWindow { get; set; }
            public ZoneVideoWindow zoneVideoWindow { get; set; }
            public string templateName { get; set; }
            public Schedule schedule { get; set; }
            public object groupIds { get; set; }
            public List<object> labels { get; set; }
        }

        public class Root_Playlists
        {
            public string stat_message { get; set; }
            public Data_Playlists data { get; set; }
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



        #endregion

        private void checkNowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            timerRefresh_Tick(null, null);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Dispose();
            Application.Exit();
        }

        private void scheduleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmSchedule frm = new();
            frm.dbConnection = GetSQLConnection();
            frm.ValidTVs.Clear();
            frm.ValidTVs.AddRange(tvs.Select(x => x.name));
            frm.ValidActions.Clear();
            frm.ValidActions.AddRange(Enum.GetNames(typeof(ScheduleActions)));
            frm.ShowDialog(this);

            //re-read the schedules into dictionary
            Action.Clear();

            using (SQLiteConnection conn = GetSQLConnection())
            {
                List<ClSchedule> _schedule = conn.Query<ClSchedule>("SELECT tv, day, time, action FROM schedule").ToList();
                libmiroppb.Log("Using following schedule:");
                foreach (ClSchedule schedule in _schedule)
                {
                    Action.Add(schedule);
                    libmiroppb.Log($"[{schedule.tv.name}, {schedule.day.ToString()}, {schedule.time.ToShortTimeString()}, {schedule.action.ToString()}]");
                }
            }
        }

        private async void timerSchedule_Tick(object sender, EventArgs e)
        {
            //should be easy peasy right?
            foreach (ClSchedule a in Action)
            {
                if (DateTime.Now.DayOfWeek == a.day && DateTime.Now.ToShortTimeString() == a.time.ToShortTimeString() && a.action == ScheduleActions.TurnOffTV)
                {
                    libmiroppb.Log("Waiting 10 seconds..."); //02.11.22 Wait between requests
                    await Task.Delay(10000);
                    libmiroppb.Log("Turning Off Tv: " + a.tv.name);
                    SendRequest("/pitv/" + a.tv.hex, Method.Post, new { status = true }); //true is off
                }
                else if (DateTime.Now.DayOfWeek == a.day && DateTime.Now.ToShortTimeString() == a.time.ToShortTimeString() && a.action == ScheduleActions.TurnOnTV)
                {
                    libmiroppb.Log("Waiting 10 seconds..."); //02.11.22 Wait between requests
                    await Task.Delay(10000);
                    libmiroppb.Log("Turning On Tv: " + a.tv.name);
                    SendRequest("/pitv/" + a.tv.hex, Method.Post, new { status = false }); //false is on
                }
                else if (DateTime.Now.DayOfWeek == a.day && DateTime.Now.ToShortTimeString() == a.time.ToShortTimeString() && a.action == ScheduleActions.Reboot)
                {
                    libmiroppb.Log("Waiting 10 seconds...");
                    await Task.Delay(10000);
                    libmiroppb.Log("Rebooting TV: " + a.tv.name);
                    SendRequest("/pishell/" + a.tv.hex, Method.Post, new { cmd = "shutdown -r now" });
                }
            }
        }

        private void showPlayerIDsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FrmPlayers frm = new();
            string json = SendRequest("/players", Method.Get, null);
            Root_Player rp = JsonConvert.DeserializeObject<Root_Player>(json);
            foreach (Object obj in rp.data.objects)
            {
                frm.DgvPlayers.Rows.Add(obj.name, obj._id);
                libmiroppb.Log("Showing player name: " + obj.name + ", id:" + obj._id);
            }
            frm.ShowDialog();
        }

        private async void onToolStripMenuItem_Click(object sender, EventArgs e)
        {
            libmiroppb.Log("Turning all On");
            foreach (ClTV tv in tvs)
            {
                libmiroppb.Log("Waiting 10 seconds..."); //02.11.22 Wait between requests...
                await Task.Delay(10000);
                libmiroppb.Log("Turning On Tv: " + tv.name);
                SendRequest("/pitv/" + tv.hex, Method.Post, new { status = false }); //false is on
            }
        }

        private async void offToolStripMenuItem_Click(object sender, EventArgs e)
        {
            libmiroppb.Log("Turning all Off");
            foreach (ClTV tv in tvs)
            {
                libmiroppb.Log("Waiting 10 seconds..."); //02.11.22 Wait between requests
                await Task.Delay(10000);
                libmiroppb.Log("Turning Off Tv: " + tv.name);
                SendRequest("/pitv/" + tv.hex, Method.Post, new { status = true }); //false is on
            }
        }

        private void reDeployGroup(object sender, EventArgs e)
        {
            ToolStripItem i = (sender as ToolStripItem);
            reDeployGroup(i.Text);
        }

        private void reDeployGroup(string groupID)
        {
            //get file with KEY
            using (SQLiteConnection conn = GetSQLConnection())
            {
                ClFiles files = conn.Query<ClFiles>($"SELECT filename FROM files WHERE playlist = '{groupID}'").FirstOrDefault();
                ClDeployOptions deployOptions = new ClDeployOptions()
                {
                    assets = new string[]
                    {
                        files.filename,
                        "__" + groups.Where(x => x.name == groupID).First().name + ".json",
                        "custom_layout.html"
                    }
                };
                string group = SendRequest("/groups/" + groups.Where(x => x.name == groupID).First().hex, Method.Post, deployOptions);
                libmiroppb.Log($"Deployed {groupID}, with options:{deployOptions}, Response: {group}");
            }
        }

        private void rebootGroup(object sender, EventArgs e)
        {
            ToolStripItem i = (sender as ToolStripItem);
            rebootGroup(i.Text);
        }

        private void rebootGroup(string TVID)
        {
            libmiroppb.Log("Rebooting TV: " + tvs.Where(x => x.name == TVID).First().name);
            SendRequest("/pishell/" + tvs.Where(x => x.name == TVID).First().hex, Method.Post, new { cmd = "shutdown -r now" });
        }

        private async Task SendNotificationAsync(string text)
        {
            var values = new Dictionary<string, string>
            {
                { "apikey", Prowl },
                { "application", "PiSiagnage Watcher" },
                { "description", text },
            };

            var content = new FormUrlEncodedContent(values);
            HttpClient client = new();
            var response = await client.PostAsync("https://api.prowlapp.com/publicapi/add", content);

            var responseString = await response.Content.ReadAsStringAsync();
        }

        SQLiteConnection GetSQLConnection()
        {
            return new SQLiteConnection("Data Source=" + Application.StartupPath + "\\db.db;Version=3;");
        }
    }
}
