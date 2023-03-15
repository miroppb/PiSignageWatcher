using Dapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using miroppb;
using MySqlConnector;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	public partial class FrmMain : Form
	{
		private string APIUrl = null;
		private string GDrive = null;
		private string Prowl = null;
		protected string token = "";
		private List<ClGroups> groups = new List<ClGroups>();
		private List<ClPlaylists> playlists = new List<ClPlaylists>();
		private List<ClSchedule> Action = new List<ClSchedule>();
		List<ClTV> tvs = new List<ClTV>();
		private DateTime LastUpdated = DateTime.MinValue;

		const string announcements_id = Config.announcements_id;

		public FrmMain()
		{
			InitializeComponent();

			libmiroppb.Log("Welcome to PiSignage Watcher! ~uWu~");

			try
			{
				using (MySqlConnection conn = Secrets.GetConnectionString())
				{
					conn.Open();
					conn.Close();
				}
			}
			catch { MessageBox.Show("There is no db connection. Exiting application..."); Application.Exit(); }

			PopulateData();

			System.Windows.Forms.Timer timerRefreshToken = new();
			timerRefreshToken.Interval = 30 * 60 * 1000;
			timerRefreshToken.Tick += TimerRefreshToken_Tick;
			TimerRefreshToken_Tick(null, null);

#if !DEBUG
            timerRefresh_Tick(null, null);
#endif
		}

		private async void TimerRefreshToken_Tick(object sender, EventArgs e)
		{
			await refreshToken();
		}

		private void PopulateData()
		{
			//settings
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				ClSettings settings = conn.Query<ClSettings>("SELECT api, gdrive, prowl FROM settings").FirstOrDefault();
				APIUrl = settings.api;
				GDrive = settings.gdrive;
				Prowl = settings.prowl;
			}

			//groups
			(contextMenuStrip1.Items[2] as ToolStripMenuItem).DropDownItems.Clear();
			(contextMenuStrip1.Items[4] as ToolStripMenuItem).DropDownItems.Clear();
			using (MySqlConnection conn = Secrets.GetConnectionString())
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
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				List<ClPlaylists> _playlists = conn.Query<ClPlaylists>("SELECT * FROM playlists").ToList();
				foreach (ClPlaylists playlist in _playlists)
					playlists.Add(playlist);
			}

			//tvs
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				List<ClTV> _tvs = conn.Query<ClTV>("SELECT * FROM tvs").ToList();
				foreach (ClTV tv in _tvs)
					tvs.Add(tv);
			}

			//read schedules into dictionary
			using (MySqlConnection conn = Secrets.GetConnectionString())
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

		private async Task<bool> refreshToken([CallerMemberName] string sender = null)
		{
			ClSettings _settings = null;
			using (MySqlConnection conn = Secrets.GetConnectionString())
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
			while (json == null)
			{
				libmiroppb.Log("Session not provided, retrying after 1 minute...");
				//retrying after a minute... //10.15.22
				await Task.Delay(60000);
				json = SendRequest("/session", Method.Post, data, false);
			}

			try
			{
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
				LastUpdated = DateTime.Now;
				libmiroppb.Log($"Called from {sender}. Refreshed token: {token}");
				return true;
			}
			catch (Exception ex)
			{
				libmiroppb.Log($"Called from {sender}. Refreshed not refreshed. Something failed: {ex.Message}");
				return false;
			}
		}

		public async void timerRefresh_Tick(object sender, EventArgs e)
		{
			//get list of files
			libmiroppb.Log("Getting list of Google Drive files...");
			Dictionary<string, string> gd_files = GetGoogleDriveFiles();
			if (gd_files == null) { return; }
			else { foreach (string file in gd_files.Keys) { libmiroppb.Log(file); } } //print each GD file to log

			List<ClFiles> _files = null;
			using (MySqlConnection conn = Secrets.GetConnectionString())
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

					if (File.Exists(file.Key))
					{
						File.Move(file.Key, file.Key.Replace(",", ""));

						//and then upload to pisignage
						string up = "";
						while (up == "" || up == null) //1.20.22 In case uploading fails (for unknown reason)
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
						using (MySqlConnection conn = Secrets.GetConnectionString())
						{
							conn.Execute($"INSERT INTO files VALUES('{file.Key}', '{pl}');");
						}
						libmiroppb.Log("Added " + file.Key + " to the database");

						groups.Where(x => x.name == pl).First().changed = true;

						timerRefresh.Interval = 60 * 60 * 1000;
					}
					else
					{
						//file doesn't exist for some reason...
						libmiroppb.Log("File doesn't exist. Will try to redownload next time...");
						timerRefresh.Interval = 60 * 1000; //01.08.23 Try a minute later, not an hour later
					}
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

					//and database
					using (MySqlConnection conn = Secrets.GetConnectionString())
					{
						conn.Execute($"DELETE FROM files WHERE filename = '{a}'");
					}
					libmiroppb.Log("Deleted file " + a + " from database");
				}
			}
			//Deploy the group that was changed
			foreach (ClGroups group in groups.Where(x => x.changed))
			{
				await Wait(30);
				using (MySqlConnection conn = Secrets.GetConnectionString())
				{
					ClFiles files = conn.Query<ClFiles>($"SELECT filename FROM files WHERE playlist = '{group.name}'").FirstOrDefault();
					ClDeployOptions deployOptions = null;
					try
					{
						deployOptions = new ClDeployOptions()
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
					catch (Exception ex)
					{
						libmiroppb.Log($"Deployed failed. Message: {ex.Message}, deployOptions: {files.filename}, __{groups.Where(x => x.name == group.name).First().name}.json, {group.hex}");
					}
				}
			}
			groups.ForEach(x => x.changed = false);
		}

		private Dictionary<string, string> GetGoogleDriveFiles()
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

		private string SendRequest(string url, Method method, object json, bool sendtoken = true)
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
			frm.ValidTVs.Clear();
			frm.ValidTVs.AddRange(tvs.Select(x => x.name));
			frm.ValidActions.Clear();
			frm.ValidActions.AddRange(Enum.GetNames(typeof(ScheduleActions)));
			frm.ShowDialog(this);

			//re-read the schedules into dictionary
			Action.Clear();

			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				List<ClSchedule> _schedule = conn.Query<ClSchedule>("SELECT tv, day, time, action FROM schedule").ToList();
				libmiroppb.Log("Using following schedule:");
				foreach (ClSchedule schedule in _schedule)
				{
					Action.Add(schedule);
					libmiroppb.Log($"[{schedule.tv.name}, {schedule.day}, {schedule.time.ToShortTimeString()}, {schedule.action}]");
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
					libmiroppb.Log("Turning Off Tv: " + a.tv.name);
					SendRequest("/pitv/" + a.tv.hex, Method.Post, new { status = true }); //true is off
					await Wait();
				}
				else if (DateTime.Now.DayOfWeek == a.day && DateTime.Now.ToShortTimeString() == a.time.ToShortTimeString() && a.action == ScheduleActions.TurnOnTV)
				{
					if ((bool)CheckOnlineStatus(a.tv.name))
					{
						libmiroppb.Log("Turning On Tv: " + a.tv.name);
						SendRequest("/pitv/" + a.tv.hex, Method.Post, new { status = false }); //false is on
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.tv.name} is offline");
				}
				else if (DateTime.Now.DayOfWeek == a.day && DateTime.Now.ToShortTimeString() == a.time.ToShortTimeString() && a.action == ScheduleActions.Reboot)
				{
					if ((bool)CheckOnlineStatus(a.tv.name))
					{
						libmiroppb.Log("Rebooting TV: " + a.tv.name);
						SendRequest("/pishell/" + a.tv.hex, Method.Post, new { cmd = "shutdown -r now" });
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.tv.name} is offline");
				}

			}
		}

		private static async Task Wait(int s = 10)
		{
			libmiroppb.Log($"Waiting {s} seconds..."); //02.11.22 Wait between requests
			await Task.Delay(1000 * s);
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

		public async void onToolStripMenuItem_Click(object sender, EventArgs e)
		{
			libmiroppb.Log("Turning all On");
			foreach (ClTV tv in tvs)
			{
				await Wait();
				libmiroppb.Log("Turning On Tv: " + tv.name);
				SendRequest("/pitv/" + tv.hex, Method.Post, new { status = false }); //false is on
			}
		}

		public async void offToolStripMenuItem_Click(object sender, EventArgs e)
		{
			libmiroppb.Log("Turning all Off");
			foreach (ClTV tv in tvs)
			{
				await Wait();
				libmiroppb.Log("Turning Off Tv: " + tv.name);
				SendRequest("/pitv/" + tv.hex, Method.Post, new { status = true }); //false is on
			}
		}

		private void reDeployGroup(object sender, EventArgs e)
		{
			ToolStripItem i = (sender as ToolStripItem);
			reDeployGroup(i.Text);
		}

		public void reDeployGroup(string groupName)
		{
			//get file with KEY
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				ClFiles files = conn.Query<ClFiles>($"SELECT filename FROM files WHERE playlist = '{groupName}'").FirstOrDefault();
				ClDeployOptions deployOptions = new ClDeployOptions()
				{
					assets = new string[]
					{
						files.filename,
						"__" + groups.Where(x => x.name == groupName).First().name + ".json",
						"custom_layout.html"
					}
				};
				string group = SendRequest("/groups/" + groups.Where(x => x.name == groupName).First().hex, Method.Post, deployOptions);
				libmiroppb.Log($"Deployed {groupName}, with options:{deployOptions.ToString()}, Response: {group}");
			}
		}

		private void rebootGroup(object sender, EventArgs e)
		{
			ToolStripItem i = (sender as ToolStripItem);
			rebootGroup(i.Text);
		}

		public void rebootGroup(string groupName)
		{
			libmiroppb.Log("Rebooting TV: " + tvs.Where(x => x.name == groupName).First().name);
			SendRequest("/pishell/" + tvs.Where(x => x.name == groupName).First().hex, Method.Post, new { cmd = "shutdown -r now" });
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

		private List<ClTVStatus> CheckOnlineTVStatus()
		{
			try
			{
				List<ClTVStatus> status = new List<ClTVStatus>();
				string json = SendRequest("/players", Method.Get, null);
				Root_Player player = JsonConvert.DeserializeObject<Root_Player>(json);
				foreach (Object o in player.data.objects)
					status.Add(new() { Name = o.name, Status = new() { IsOnline = o.isConnected, cecStatus = o.cecTvStatus } });

				return status;
			}
			catch
			{
				return null;
			}
		}

		public bool? CheckOnlineStatus(string name)
		{
			string hex = tvs.Where(x => x.name == name).FirstOrDefault().hex;
			if (hex == null)
				return CheckOnlineTVStatus().Where(x => x.Name == name).FirstOrDefault().Status.IsOnline;
			else
				return null;
		}

		public List<ClTVStatus> CheckAllDevicesOnlineStatus()
		{
			return CheckOnlineTVStatus();
		}
	}
}
