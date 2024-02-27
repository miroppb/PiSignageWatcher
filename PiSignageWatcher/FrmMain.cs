using Dapper;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	public partial class FrmMain : Form
	{
		ClSettings Settings = new();
		protected string token = "";
		private List<JSON.Pi_Groups.Datum> Groups = new();
		private List<ClSchedule> Schedules = new();
		public List<ClPlayer> Players = new();

		public List<JSON.Pi_Playlists.Datum> AllPlaylists = new();

		ToolStripMenuItem RedeployMI;
		ToolStripMenuItem RebootMI;
		ToolStripMenuItem PowerAllMI;
		ToolStripMenuItem DeployMI;


		public FrmMain()
		{
			InitializeComponent();

			libmiroppb.Log("Welcome to PiSignage Watcher! ~uWu~");

			try
			{
				using MySqlConnection conn = Secrets.GetConnectionString();
				conn.Open();
				conn.Close();
			}
			catch { MessageBox.Show("There is no db connection. Exiting application..."); Application.Exit(); }

			timerRefreshToken.Elapsed += async delegate
			{
				await RefreshToken();
			};
			timerRefreshToken.Start();
			libmiroppb.Log("Refresh timer started");

			FindMenuItems();

			PopulateData();

			timerSchedule.Elapsed += delegate
			{
				timerSchedule_Tick();
			};
			timerSchedule.Start();
			libmiroppb.Log("Schedule timer started");

			timerUploadLogs.Elapsed += delegate
			{
				libmiroppb.Log("Uploading logs");
				libmiroppb.UploadLog(Secrets.GetConnectionString().ConnectionString, "logs", true);
			};
			timerUploadLogs.Start();
			libmiroppb.Log("Upload logs timer started");

			timerRefreshFiles.Elapsed += delegate
			{
				RefreshFiles();
			};
			timerRefreshFiles.Start();
			libmiroppb.Log("Refresh Files timer started");

			timerRefreshData.Elapsed += delegate
			{
				PopulateData();
			};
			timerRefreshData.Start();
			libmiroppb.Log("Refresh Data timer started");

#if DEBUG
			//RefreshFiles();
#endif
		}

		readonly System.Timers.Timer timerRefreshToken = new()
		{
			Interval = 30 * 60 * 1000, //30 minutes
			Enabled = true
		};

		readonly System.Timers.Timer timerSchedule = new()
		{
			Interval = 60 * 1000, //1 minute
			Enabled = true
		};

		readonly System.Timers.Timer timerUploadLogs = new()
		{
			Interval = 24 * 60 * 60 * 1000, //1 day
			Enabled = true
		};

		readonly System.Timers.Timer timerRefreshFiles = new()
		{
			Interval = 60 * 60 * 1000, //1 hour
			Enabled = true
		};

		readonly System.Timers.Timer timerRefreshData = new()
		{
			Interval = 24 * 60 * 60 * 1000, //1 day
			Enabled = true
		};

		public async void PopulateData()
		{
			GetSettings();

			await RefreshToken();

			RefreshPlayers();

			RefreshGroups();

			RefreshPlaylists();

			RefreshSchedules();
		}

		private async Task<bool> RefreshToken([CallerMemberName] string sender = null)
		{
			Dictionary<string, string> data = new()
			{
				{ "email", Settings.user },
				{ "password", Settings.pass },
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
				//libmiroppb.Log($"Called from {sender}. Refreshed token: {token}");
				return true;
			}
			catch (Exception ex)
			{
				//libmiroppb.Log($"Called from {sender}. Refreshed not refreshed. Something failed: {ex.Message}");
				return false;
			}
		}

		private void GetSettings()
		{
			using MySqlConnection conn = Secrets.GetConnectionString();
			Settings = conn.Query<ClSettings>("SELECT user, pass, api, prowl, path FROM settings").FirstOrDefault();
		}

		private void RefreshPlayers()
		{
			string json = SendRequest("/players", Method.Get, null);
			Root_Player rp = JsonConvert.DeserializeObject<Root_Player>(json);
			Players.Clear();
			Players.AddRange(rp.data.objects.Select(o => new ClPlayer() { name = o.name, hex = o._id }));

			if (Players.Count > 0)
			{
				RebootMI.DropDownItems.Clear();
				foreach (ClPlayer p in Players)
				{
					RebootMI.DropDownItems.Add(p.name, null, RebootPlayer);
				}
			}
		}

		private void RefreshGroups()
		{
			string json = SendRequest("/groups", Method.Get, null);
			Groups = JsonConvert.DeserializeObject<JSON.Pi_Groups.Root>(json).data;

			if (Groups.Count > 0)
			{
				RedeployMI.DropDownItems.Clear();
				foreach (JSON.Pi_Groups.Datum group in Groups)
				{
					RedeployMI.DropDownItems.Add(group.name, null, ReDeployGroup);
				}
			}
		}

		private void RefreshPlaylists()
		{
			AllPlaylists = JsonConvert.DeserializeObject<JSON.Pi_Playlists.Root>(SendRequest("/playlists", Method.Get, null)).data;

			List<JSON.Pi_Playlists.Datum> PlaylistsWithAssets = AllPlaylists.Where(x => x.assets.Count > 0).ToList();
			if (PlaylistsWithAssets.Count > 0)
				DeployMI.DropDownItems.Clear();

			foreach (JSON.Pi_Playlists.Datum i in PlaylistsWithAssets)
			{
				DeployMI.DropDownItems.Add(i.name);
			}
			for (int a = 0; a < DeployMI.DropDownItems.Count; a++)
			{
				foreach (ClPlayer tv in Players)
				{
					ToolStripMenuItem toolStripMenuItem = new()
					{
						Tag = DeployMI.DropDownItems[a].Text,
						Text = tv.name
					};
					toolStripMenuItem.Click += DeployPlaylistToTV;
					(DeployMI.DropDownItems[a] as ToolStripMenuItem).DropDownItems.Add(toolStripMenuItem);
				}
			}
		}

		private void RefreshSchedules()
		{
			Schedules.Clear();

			using MySqlConnection conn = Secrets.GetConnectionString();
			List<ClSchedule> _schedule = conn.Query<ClSchedule>("SELECT * FROM schedule").ToList();

			libmiroppb.Log("Using following schedule:");
			foreach (ClSchedule schedule in _schedule)
			{
				schedule.player = Players.FirstOrDefault(x => x.name == schedule.name);
				Schedules.Add(schedule);
				libmiroppb.Log($"[{schedule.player.name}, {schedule.day}, {schedule.time}, {schedule.action}]");
			}
		}

		public async void RefreshFiles()
		{
			libmiroppb.Log("Checking for updates to video files");
			List<string> video_files = GetListOfFiles(Settings.path);
			foreach (string file in video_files) { libmiroppb.Log(file); }

			List<ClFiles> _files = null;
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				libmiroppb.Log("Getting list of database files...");
				_files = conn.Query<ClFiles>("SELECT * FROM files").ToList();
			}

			List<string> db_files = new();
			foreach (ClFiles file in _files) { db_files.Add(file.filename); libmiroppb.Log(file.filename); }

			//compare the db files with gd files
			foreach (string file in video_files)
			{
				string CurrentPlaylist = file.Split(' ')[0];
				//for each file that doesn't exist already (new file) and starts with a playlist "search" term
				if (!db_files.Contains(file) && AllPlaylists.Any(x => x.name == CurrentPlaylist))
				{
					libmiroppb.Log("Working on: " + file);
					_ = SendNotificationAsync("Working on " + file);

					//copy file to current dir
					File.Copy(Path.Combine(Settings.path, file), file, true);

					if (File.Exists(file))
					{
						//remove commas
						File.Move(file, file.Replace(",", ""));

						//and then attempt to upload to pisignage
						string up = UploadToPiSignage(file);

						File.Delete(file);
						libmiroppb.Log("Deleted local file: " + file);

						PostUploadtoPiSignage(file, up);

						await Task.Delay(1000);

						//we'll have only 1 file per TV, for now
						Root_Files rf = GetInfoAboutFileonPiSignage(file);

						//error (rf null). See the best way to resolve this
						//add current file to playlist
						
						//we're using the first WORD to determine which playlist the file goes to
						//this file will effectively replace the current file in the playlist, keeping all zones same
						AddFileToPlaylistonPiSignage(CurrentPlaylist, file, Convert.ToInt32(rf.data.dbdata.duration));

						//add file to database
						using (MySqlConnection conn = Secrets.GetConnectionString())
						{
							conn.Execute($"INSERT INTO files VALUES('{file}', '{CurrentPlaylist}');");
						}
						libmiroppb.Log("Added " + file + " to the database");

						DeployPlaylistToGroup(CurrentPlaylist, CurrentPlaylist);

						timerRefreshFiles.Interval = 60 * 60 * 1000;
					}
					else
					{
						//file doesn't exist for some reason...
						libmiroppb.Log("File doesn't exist. Will try to redownload next time...");
						timerRefreshFiles.Interval = 60 * 1000; //01.08.23 Try a minute later, not an hour later
					}
				}
			}
			foreach (string a in db_files)
			{
				//else if db file exists but gd doesn't
				if (!video_files.Contains(a))
				{
					DeleteFileFromPiSignageAndDB(a);
				}
			}
		}

		private void DeleteFileFromPiSignageAndDB(string a)
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

		private void AddFileToPlaylistonPiSignage(string pl, string file, int duration)
		{
			RefreshPlaylists();

			//get current playlist
			JSON.Pi_Playlists.Datum Selected = AllPlaylists.First(x => x.name == pl);
			//All we're doing is replacing the filename, and keeping the other zones same
			Asset_Files af = new()
			{
				filename = file,
				dragSelected = false,
				fullscreen = Selected.assets.Count == 0 || Selected.assets[0].fullscreen,
				isVideo = true,
				selected = true,
				duration = duration,
				bottom = Selected.assets.Count > 0 ? Selected.assets[0].bottom : "",
				side = Selected.assets.Count > 0 ? Selected.assets[0].side : "",
				zone4 = Selected.assets.Count > 0 ? Selected.assets[0].zone4 : "",
				zone5 = Selected.assets.Count > 0 ? Selected.assets[0].zone5 : ""
			};
			string playlistResponse = SendRequest("/playlists/" + pl, Method.Post, new { Selected.templateName, assets = new object[] { af } });
			libmiroppb.Log("Added to playlist " + pl + ": " + file + ", Response: " + playlistResponse);
		}

		private Root_Files GetInfoAboutFileonPiSignage(string file)
		{
			string json = SendRequest("/files/" + file, Method.Get, null);
			libmiroppb.Log("Getting: " + file + ", Response: " + json);
			Root_Files rf = JsonConvert.DeserializeObject<Root_Files>(json);
			return rf;
		}

		private void PostUploadtoPiSignage(string file, string up)
		{
			Root_Files_Upload rfu = JsonConvert.DeserializeObject<Root_Files_Upload>(up);
			//process upload
			string post = SendRequest("/postupload", Method.Post, new { files = rfu.data });
			libmiroppb.Log("PostUpload: " + file + ", Response: " + post);
		}

		private string UploadToPiSignage(string file)
		{
			string up = "";
			while (up == "" || up == null) //1.20.22 In case uploading fails (for unknown reason)
			{
				up = Upload("/files", file);
				libmiroppb.Log("Upload attempt: " + file + ", Response: " + up);
				_ = SendNotificationAsync("Upload attempt " + file);
			}
			libmiroppb.Log("Uploaded: " + file + ", Response: " + up);
			_ = SendNotificationAsync("Uploaded " + file);
			return up;
		}

		private string Upload(string url, string filename)
		{
			RestClient restClient = new(Settings.api);
			RestRequest restRequest = new(url + "?token=" + token)
			{
				RequestFormat = DataFormat.Json,
				Method = Method.Post
			};
			restRequest.AddHeader("Content-Type", "multipart/form-data");
			restRequest.AddFile("content", filename);
			var response = restClient.Execute(restRequest);
			return response.Content;
		}

		private string SendRequest(string url, Method method, object json, bool sendtoken = true)
		{
			RestClient restClient = new(Settings.api);
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

		private static List<string> GetListOfFiles(string path) => Directory.GetFiles(path, "*.mp4", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToList();

		private void checkNowToolStripMenuItem_Click(object sender, EventArgs e) => RefreshFiles();

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			notifyIcon1.Dispose();
			Application.Exit();
		}

		private void scheduleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FrmSchedule frm = new()
			{
				ValidPlayers = Players.Select(x => x.name).ToList(),
				ValidActions = GetEnumList<ScheduleActions>()
			};
			frm.ShowDialog(this);

			RefreshSchedules();
		}

		private async void timerSchedule_Tick()
		{
			//should be easy peasy right?
			foreach (ClSchedule a in Schedules)
			{
				if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.day) && DateTime.Now.ToShortTimeString() == a.time && Enum.Parse<ScheduleActions>(a.action) == ScheduleActions.TurnOffTV)
				{
					libmiroppb.Log($"Turning off TV {a.player.name}");
					await SendNotificationAsync($"Turning off TV {a.player.name}");
					SendRequest("/pitv/" + a.player.hex, Method.Post, new { status = true }); //true is off
					await Wait();
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.day) && DateTime.Now.ToShortTimeString() == a.time && Enum.Parse<ScheduleActions>(a.action) == ScheduleActions.TurnOnTV)
				{
					bool? t = CheckOnlineStatus(a.player.name);
					if (t != null && t.Value)
					{
						libmiroppb.Log($"Turning On TV {a.player.name}");
						await SendNotificationAsync($"Turning On TV {a.player.name}");
						SendRequest("/pitv/" + a.player.hex, Method.Post, new { status = false }); //false is on
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.player.name} is offline");
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.day) && DateTime.Now.ToShortTimeString() == a.time && Enum.Parse<ScheduleActions>(a.action) == ScheduleActions.Reboot)
				{
					bool? t = CheckOnlineStatus(a.player.name);
					if (t != null && t.Value)
					{
						libmiroppb.Log($"Rebooting TV {a.player.name}");
						await SendNotificationAsync($"Rebooting TV {a.player.name}");
						SendRequest("/pishell/" + a.player.hex, Method.Post, new { cmd = "shutdown -r now" });
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.player.name} is offline");
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.day) && DateTime.Now.ToShortTimeString() == a.time && Enum.Parse<ScheduleActions>(a.action) == ScheduleActions.DeployPlaylist)
				{
					bool? t = CheckOnlineStatus(a.player.name);
					if (t != null && t.Value)
					{
						libmiroppb.Log($"Deploying: {a.subaction} to {a.name}");
						await SendNotificationAsync($"Deploying: {a.subaction} to {a.name}");
						DeployPlaylistToGroup(a.subaction, a.name);
						await Wait();
					}
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

			foreach (ClPlayer p in Players)
			{
				frm.DgvPlayers.Rows.Add(p.name, p.hex);
				libmiroppb.Log("Showing player name: " + p.name + ", id:" + p.hex);
			}
			frm.ShowDialog();
		}

		public async void onToolStripMenuItem_Click(object sender, EventArgs e)
		{
			libmiroppb.Log("Turning all On");
			foreach (ClPlayer tv in Players)
			{
				await Wait();
				libmiroppb.Log("Turning On Tv: " + tv.name);
				SendRequest("/pitv/" + tv.hex, Method.Post, new { status = false }); //false is on
			}
		}

		public async void offToolStripMenuItem_Click(object sender, EventArgs e)
		{
			libmiroppb.Log("Turning all Off");
			foreach (ClPlayer tv in Players)
			{
				await Wait();
				libmiroppb.Log("Turning Off Tv: " + tv.name);
				SendRequest("/pitv/" + tv.hex, Method.Post, new { status = true }); //false is on
			}
		}

		private void ReDeployGroup(object sender, EventArgs e)
		{
			ToolStripItem i = sender as ToolStripItem;
			DeployPlaylistToGroup(i.Text, i.Text);
		}

		private void DeployPlaylistToTV(object sender, EventArgs e)
		{
			ToolStripItem i = sender as ToolStripItem;
			string p = i.Tag as string;
			string t = i.Text;
			DeployPlaylistToGroup(p, t);
		}

		public void DeployPlaylistToGroup(string p, string g)
		{
			RefreshPlaylists();

			//get assets in playlist and put into cldeployoptions
			JSON.Pi_Playlists.Datum SelectedPlaylist = AllPlaylists.First(x => x.name == p);
			List<JSON.Pi_Playlists.Asset> assets = SelectedPlaylist.assets;
			List<string> col = new();
			foreach (JSON.Pi_Playlists.Asset a in assets)
			{
				col.AddRange(new string[] { a.filename, a.bottom, a.side, a.zone4, a.zone5 });
			}
			col.AddRange(new string[] { SelectedPlaylist.templateName, $"__{SelectedPlaylist.name}.json" });
			ClDeployOptions options = new()
			{
				assets = col.ToArray(),
				deploy = true
			};

			string group = SendRequest("/groups/" + Groups.Where(x => x.name == g).First()._id, Method.Post, options);
			libmiroppb.Log($"Deployed {g}, with options: {options}, Response: {group}");
		}

		private void RebootPlayer(object sender, EventArgs e)
		{
			ToolStripItem i = sender as ToolStripItem;
			RebootPlayer(i.Text);
		}

		public void RebootPlayer(string groupName)
		{
			libmiroppb.Log("Rebooting TV: " + Players.Where(x => x.name == groupName).First().name);
			SendRequest("/pishell/" + Players.Where(x => x.name == groupName).First().hex, Method.Post, new { cmd = "shutdown -r now" });
		}

		private void FindMenuItems()
		{
			foreach (ToolStripMenuItem a in contextMenuStrip1.Items)
			{
				switch (a.Text)
				{
					case "Reboot":
						RebootMI = a;
						break;
					case "Re-Deploy":
						RedeployMI = a;
						break;
					case "Power All":
						PowerAllMI = a;
						break;
					case "Deploy":
						DeployMI = a;
						break;
				}
			}
		}

		private async Task SendNotificationAsync(string text)
		{
			var values = new Dictionary<string, string>
			{
				{ "apikey", Settings.prowl },
				{ "application", "PiSiagnage Watcher" },
				{ "description", text },
			};

			var content = new FormUrlEncodedContent(values);
			HttpClient client = new();
			var response = await client.PostAsync("https://api.prowlapp.com/publicapi/add", content);
			_ = await response.Content.ReadAsStringAsync();
		}

		private List<ClTVStatus> CheckOnlineTVStatus()
		{
			try
			{
				List<ClTVStatus> status = new();
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
			string hex = Players.Where(x => x.name == name).FirstOrDefault().hex;
			if (hex != null)
				return CheckOnlineTVStatus().Where(x => x.Name == name).FirstOrDefault().Status.IsOnline;
			else
				return null;
		}

		public List<ClTVStatus> CheckAllDevicesOnlineStatus() => CheckOnlineTVStatus();

		public static List<string> GetEnumList<T>() where T : Enum => Enum.GetNames(typeof(T)).ToList();
	}
}
