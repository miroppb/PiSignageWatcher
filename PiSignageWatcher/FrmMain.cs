﻿using Google.Apis.Auth.OAuth2;
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

namespace PiSignageWatcher
{
    public partial class FrmMain : Form
    {
        public const string APIUrl = Config.APIUrl;
        protected string token = "";
        protected string LEFT_group = Config.LEFT_group;

        SQLiteConnection dbConnection;

        const string announcements_id = Config.announcements_id;

        public FrmMain()
        {
            InitializeComponent();

            libmiroppb.Log("Welcome to PiSignage Watcher! ~uWu~");

            dbConnection = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\db.db;Version=3;");

            timerRefresh_Tick(null, null);
        }

        private bool refreshToken()
        {
            DataTable dt = GetDataTable("SELECT user, pass FROM settings");

            Dictionary<string, string> data = new Dictionary<string, string>
            {
                { "email", dt.Rows[0].ItemArray[0].ToString() },
                { "password", dt.Rows[0].ItemArray[1].ToString() },
                { "getToken", "true" }
            };

            string json = SendRequest("/session", Method.POST, data);

            Root_Session t = JsonConvert.DeserializeObject<Root_Session>(json);
            token = t.token;
            libmiroppb.Log("Refreshed token: " + token);
            return true;
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            //lets get authenticated
            if (refreshToken())
            {
                //get list of files
                DataTable dt = GetDataTable("SELECT * FROM files");
                libmiroppb.Log("Getting list of database files...");

                Dictionary<string, string> gd_files = GetGoogleDriveFiles();
                libmiroppb.Log("Getting list of Google Drive files...");

                List<string> db_files = new List<string>();
                libmiroppb.Log("DB files:");
                foreach (DataRow dr in dt.Rows) { db_files.Add(dr.ItemArray[0].ToString()); libmiroppb.Log(dr.ItemArray[0].ToString()); }

                //compare the db files with gd files
                foreach (KeyValuePair<string, string> file in gd_files)
                {
                    //for each file that doesn't exist already (new file)
                    if (!db_files.Contains(file.Key) && file.Key.StartsWith("LEFT"))  //change whenever
                    {
                        libmiroppb.Log("Working on: " + file.Key);

                        //if gd file exists but db doesnt, we need to download
                        GoogleDownloadFile(file.Value, file.Key);
                        libmiroppb.Log("Downloaded: " + file.Key);

                        //and then upload to pisignage
                        string up = Upload("/files", file.Key);
                        libmiroppb.Log("Uploaded: " + file.Key + ", Response: " + up);


                        Root_Files_Upload rfu = JsonConvert.DeserializeObject<Root_Files_Upload>(up);
                        //process upload
                        string post = SendRequest("/postupload", Method.POST, new { files = rfu.data });
                        libmiroppb.Log("PostUpload: " + file.Key + ", Response: " + post);

                        //we'll have only 1 file per TV, for now
                        string json = SendRequest("/files/" +file.Key, Method.GET, null);
                        libmiroppb.Log("Getting: " + file.Key + ", Response: " + json);
                        Root_Files rf = JsonConvert.DeserializeObject<Root_Files>(json);
                        //add current file to playlist
                        Asset_Files af = new Asset_Files
                        {
                            filename = file.Key,
                            dragSelected = false,
                            fullscreen = true,
                            isVideo = true,
                            selected = true,
                            duration = Convert.ToInt32(rf.data.dbdata.duration)
                        };
                        object[] resArray = new object[] { af };
                        string playlist = SendRequest("/playlists/LEFT_TV", Method.POST, new { assets = resArray });
                        libmiroppb.Log("Added to playlist LEFT: " + file.Key + ", Response: " + playlist);

                        //add file to database
                        _ = ExecuteNonQuery("INSERT INTO files VALUES(\"" + file.Key + "\");");
                        libmiroppb.Log("Added " + file.Key + " to the database");
                    }
                }
                foreach (string a in db_files)
                {
                    //else if db file exists but gd doesn't
                    if (!gd_files.ContainsKey(a))
                    {
                        //remove db file from pisignage
                        string files = SendRequest("/files/" + a, Method.DELETE, null);
                        libmiroppb.Log("Deleted file: " + a + ", Response: " + files);

                        //and database
                        _ = ExecuteNonQuery("DELETE FROM files WHERE filename = \"" + a + "\"");
                        libmiroppb.Log("Deleted file " + a + " from database");
                    }
                }
                //Deploy
                //LEFT
                string groups = SendRequest("/groups/" + LEFT_group, Method.POST, new { deploy = true });
                libmiroppb.Log("Deployed LEFT, Response: " + groups);
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

            // Define parameters of request.
            FilesResource.ListRequest listRequest = service.Files.List();
            //FilesResource.ListRequest listRequest = service.
            listRequest.PageSize = 10;
            listRequest.Fields = "nextPageToken, files(id, name)";
            listRequest.Q = "'" + announcements_id + "' in parents";

            // List files.
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
            using (System.IO.FileStream file = new System.IO.FileStream(saveTo, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }

        private string Upload(string url, string filename)
        {
            RestClient restClient = new RestClient(APIUrl);
            RestRequest restRequest = new RestRequest(url + "?token=" + token);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.Method = Method.POST;
            restRequest.AddHeader("Content-Type", "multipart/form-data");
            restRequest.AddFile("content", filename);
            var response = restClient.Execute(restRequest);
            return response.Content;
        }

        private string SendRequest(string url, RestSharp.Method method, object json)
        {
            RestClient restClient = new RestClient(APIUrl);
            RestRequest restRequest = new RestRequest(url + "?token=" + token);
            restRequest.RequestFormat = DataFormat.Json;
            restRequest.Method = method;
            if (json != null)
                restRequest.AddJsonBody(json);
            var response = restClient.Execute(restRequest);
            return response.Content;
        }

        #region SQLite code
        public DataTable GetDataTable(string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                SQLiteConnection conn = new SQLiteConnection(dbConnection);
                conn.Open();
                SQLiteCommand comm = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = comm.ExecuteReader();
                dt.Load(reader);
                reader.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
            return dt;
        }

        public int ExecuteNonQuery(string sql)
        {
            SQLiteConnection conn = new SQLiteConnection(dbConnection);
            conn.Open();
            SQLiteCommand comm = new SQLiteCommand(conn);
            comm.CommandText = sql;
            int rowsUpdated = comm.ExecuteNonQuery();
            conn.Close();
            return rowsUpdated;
        }

        public int ExecuteNonQueryWithBlob(string sql, string blobFieldName, byte[] blob)
        {
            SQLiteConnection con = new SQLiteConnection(dbConnection);
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = String.Format(sql);
            SQLiteParameter param = new SQLiteParameter("@" + blobFieldName, System.Data.DbType.Binary);
            param.Value = blob;
            cmd.Parameters.Add(param);
            con.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception exc1)
            {
                MessageBox.Show(exc1.Message);
            }
            con.Close();
            return 0;
        }

        public bool Update(string tableName, Dictionary<string, string> data, string where)
        {
            string vals = "";
            bool returnCode = true;
            if (data.Count >= 1)
            {
                foreach (KeyValuePair<string, string> val in data)
                {
                    vals += String.Format(" {0} = '{1}',", val.Key.ToString(), val.Value.ToString());
                }
                vals = vals.Substring(0, vals.Length - 1);
            }
            try
            {
                this.ExecuteNonQuery(String.Format("UPDATE {0} SET {1} WHERE {2}", tableName, vals, where));
            }
            catch
            {
                returnCode = false;
            }
            return returnCode;
        }

        public bool Delete(string tableName, string where)
        {
            bool returnCode = true;
            try
            {
                this.ExecuteNonQuery(String.Format("DELETE FROM {0} WHERE {1}", tableName, where));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                returnCode = false;
            }
            return returnCode;
        }

        public bool Insert(string tableName, Dictionary<string, string> data)
        {
            string columns = "";
            string values = "";
            bool returnCode = true;
            foreach (KeyValuePair<string, string> val in data)
            {
                columns += String.Format(" {0},", val.Key.ToString());
                values += String.Format(" '{0}',", val.Value);
            }
            columns = columns.Substring(0, columns.Length - 1);
            values = values.Substring(0, values.Length - 1);
            try
            {
                this.ExecuteNonQuery(String.Format("INSERT INTO {0}({1}) VALUES({2});", tableName, columns, values));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                returnCode = false;
            }
            return returnCode;
        }
        public void Prepare(string sql, List<SQLiteParameter> data)
        {
            SQLiteConnection conn = new SQLiteConnection(dbConnection);
            conn.Open();
            SQLiteCommand comm = new SQLiteCommand(conn);
            comm.CommandText = sql;
            for (int c = 0; c < data.Count(); c++)
                comm.Parameters.Add(data[c]);
            comm.ExecuteNonQuery();
            conn.Close();
        }
        #endregion

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

        #endregion
    }
}
