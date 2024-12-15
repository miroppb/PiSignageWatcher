using Dapper;
using miroppb;
using MySqlConnector;
using Newtonsoft.Json;
using OtpNet;
using PiSignageWatcher.JSON;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	[SupportedOSPlatform("windows")]
	public partial class FrmMain : Form
	{
		private ClSettings Settings = new();
		protected string token = "";
		private List<Pi_Groups.Datum> Groups = [];
		private readonly List<ClSchedule> Schedules = [];
		public List<ClPlayer> Players = [];
		private List<Layouts> AllLayouts = [];
		private List<string> DBFiles = [];

		public List<Pi_Playlists.Datum> AllPlaylists = [];

		ToolStripMenuItem RedeployMI;
		ToolStripMenuItem RebootMI;
		ToolStripMenuItem PowerAllMI;
		ToolStripMenuItem DeployMI;

		public FrmMain()
		{
			InitializeComponent();

			Libmiroppb.Log("Welcome to PiSignage Watcher! ~uWu~");

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
			Libmiroppb.Log("Refresh timer started");

			FindMenuItems();

			PopulateData();

			timerSchedule.Elapsed += delegate
			{
				TimerSchedule_Tick();
			};
			timerSchedule.Start();
			Libmiroppb.Log("Schedule timer started");

			timerUploadLogs.Elapsed += delegate
			{
				Libmiroppb.Log("Uploading logs");
				Libmiroppb.UploadLogAsync(Secrets.GetConnectionString().ConnectionString, true);
			};
			timerUploadLogs.Start();
			Libmiroppb.Log("Upload logs timer started");

			timerRefreshFiles.Elapsed += delegate
			{
				RefreshFiles();
			};
			timerRefreshFiles.Start();
			Libmiroppb.Log("Refresh Files timer started");

			timerRefreshData.Elapsed += delegate
			{
				PopulateData();
			};
			timerRefreshData.Start();
			Libmiroppb.Log("Refresh Data timer started");

#if DEBUG
			//if (Environment.GetCommandLineArgs().Length == 2)
			//{
			//	RefreshFiles(Environment.GetCommandLineArgs()[1]);
			//}
			RefreshFiles();
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
			var secretKey = Base32Encoding.ToBytes(Settings.Otp);
			var totp = new Totp(secretKey);
			var otp = totp.ComputeTotp();

			Dictionary<string, string> data = new()
			{
				{ "email", Settings.User },
				{ "password", Settings.Pass },
				{ "getToken", "true" },
				{ "code", otp }
			};

			string json = SendRequest("/session", Method.Post, data, false);
			while (json == null)
			{
				Libmiroppb.Log("Session not provided, retrying after 1 minute...");
				//retrying after a minute... //10.15.22
				await Task.Delay(60000);
				json = SendRequest("/session", Method.Post, data, false);
			}

			try
			{
				Root_Session t = JsonConvert.DeserializeObject<Root_Session>(json);
				while (t == null)
				{
					Libmiroppb.Log("Token not provided, retrying after 1 minute...");
					//retrying after a few minutes... //2.24.22
					await Task.Delay(60000);
					json = SendRequest("/session", Method.Post, data, false);
					t = JsonConvert.DeserializeObject<Root_Session>(json);
				}
				token = t.token;
				Libmiroppb.Log($"Called from {sender}. Refreshed token: {token}");
				return true;
			}
			catch
			{
				//Libmiroppb.Log($"Called from {sender}. Refreshed not refreshed. Something failed: {ex.Message}");
				return false;
			}
		}

		private void GetSettings()
		{
			using MySqlConnection conn = Secrets.GetConnectionString();
			Settings = conn.Query<ClSettings>("SELECT user, pass, api, prowl, path, otp FROM settings").FirstOrDefault();
		}

		private void RefreshPlayers()
		{
			string json = SendRequest("/players", Method.Get, null);
			Root_Player rp = JsonConvert.DeserializeObject<Root_Player>(json);
			Players.Clear();
			Players.AddRange(rp.data.objects.Select(o => new ClPlayer() { Name = o.name, Hex = o._id }));

			if (Players.Count > 0)
			{
				RebootMI.DropDownItems.Clear();
				foreach (ClPlayer p in Players)
				{
					RebootMI.DropDownItems.Add(p.Name, null, RebootPlayer);
				}
			}
		}

		private void RefreshGroups()
		{
			string json = SendRequest("/groups", Method.Get, null);
			Groups = JsonConvert.DeserializeObject<Pi_Groups.Root>(json).data;

			if (Groups.Count > 0)
			{
				RedeployMI.DropDownItems.Clear();
				foreach (Pi_Groups.Datum group in Groups)
				{
					RedeployMI.DropDownItems.Add(group.name, null, ReDeployGroup);
				}
			}
		}

		private void RefreshPlaylists()
		{
			AllPlaylists = JsonConvert.DeserializeObject<Pi_Playlists.Root>(SendRequest("/playlists", Method.Get, null)).data;

			List<Pi_Playlists.Datum> PlaylistsWithAssets = AllPlaylists.Where(x => x.assets.Count > 0).ToList();
			if (PlaylistsWithAssets.Count > 0)
				DeployMI.DropDownItems.Clear();

			foreach (Pi_Playlists.Datum i in PlaylistsWithAssets)
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
						Text = tv.Name
					};
					toolStripMenuItem.Click += DeployPlaylistToTV;
					(DeployMI.DropDownItems[a] as ToolStripMenuItem).DropDownItems.Add(toolStripMenuItem);
				}
			}
		}

		void RefreshLayouts()
		{
			using MySqlConnection conn = Secrets.GetConnectionString();
			var temp = conn.Query<Layouts>("SELECT * FROM layouts");
			AllLayouts = temp.ToList();
		}

		private void RefreshSchedules()
		{
			Schedules.Clear();

			using MySqlConnection conn = Secrets.GetConnectionString();
			List<ClSchedule> _schedule = conn.Query<ClSchedule>("SELECT * FROM schedule").ToList();

			Libmiroppb.Log("Using following schedule:");
			foreach (ClSchedule schedule in _schedule)
			{
				schedule.Player = Players.FirstOrDefault(x => x.Name == schedule.Name);
				Schedules.Add(schedule);
				Libmiroppb.Log($"[{schedule.Player.Name}, {schedule.Day}, {schedule.Time}, {schedule.Action}]");
			}
		}

		private void RefreshListOfFiles()
		{
			string json = SendRequest("/files", Method.Get, null);
			DBFiles = JsonConvert.DeserializeObject<Pi_Files.Root>(json).Data.Dbdata.Where(x => x.Type == "video").Select(x => x.Name).ToList();
		}

		public async void RefreshFiles(string path = "")
		{
			if (path == "") { path = Settings.Path; }
#if DEBUG
			path = "";
#endif
			Directory.SetCurrentDirectory(AppContext.BaseDirectory); //06.06.24 Because the application can be started from scheduler

			Libmiroppb.Log("Checking for updates to video files");
#if DEBUG
			List<string> video_files = ["TEST 'FILE.mp4"];
#else
			List<string> video_files = GetListOfFiles(path);
#endif
			for (int i = 0; i < video_files.Count; i++)
			{
				File.Move(Path.Combine(path, video_files[i]), Path.Combine(path, video_files[i].ReplaceInvalidChars())); //remove special chars
				video_files[i] = video_files[i].ReplaceInvalidChars();
			}

			foreach (string file in video_files) { Libmiroppb.Log(file); }

			Libmiroppb.Log("Getting list of assets...");
			RefreshListOfFiles();

			foreach (string i in DBFiles) { Libmiroppb.Log(i); }

			for (int v = 0; v < video_files.Count; v++)
			{
				//check if file is over 1GB
				FileInfo fileInfo = new(video_files[v]);

				long fileSizeInBytes = fileInfo.Length;
				double fileSizeInGB = fileSizeInBytes / (1024.0 * 1024.0 * 1024.0);

				if (fileSizeInGB > 1.0)
				{
					// File is larger than 1GB; proceed with optimization
					Console.WriteLine($"File size: {fileSizeInGB:F2} GB");
					string directoryPath = Path.GetDirectoryName(video_files[v]);
					string originalFileName = Path.GetFileNameWithoutExtension(video_files[v]);
					string extension = Path.GetExtension(video_files[v]);

					// Create the new filename with "_less" appended
					string newFileName = $"{originalFileName}_less{extension}";

					// Combine the directory path and new filename
					string newFilePath = Path.Combine(directoryPath, newFileName);
					ProcessStartInfo startInfo = new(@"C:\ffmpeg\ffmpeg.exe")
					{
						Arguments = $"-i \"{video_files[v]}\" -b:v 4M -c:v libx264 -strict experimental \"{newFilePath}\"",
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					};

					using (Process process = new() { StartInfo = startInfo })
					{
						process.Start();
						process.WaitForExit();
					}

					File.Delete(video_files[v]);
					video_files[v] = newFilePath;
				}
				else
				{
					Console.WriteLine("File size is within the desired limit.");
				}
			}

			//compare the db files with gd files
			foreach (string file in video_files)
			{
				string CurrentPlaylist = file.Split(' ')[0];
				//for each file that doesn't exist already (new file) and starts with a playlist "search" term
				if (!DBFiles.Contains(file) && AllPlaylists.Any(x => x.name == CurrentPlaylist))
				{
					Libmiroppb.Log("Working on: " + file);
					_ = SendNotificationAsync("Working on " + file);

					File.Copy(Path.Combine(path, file), file, true); //copy to current dir
					await Task.Delay(3000);

					if (File.Exists(file))
					{
						//and then attempt to upload to pisignage
						string up = UploadToPiSignage(file);

						File.Delete(file);
						Libmiroppb.Log("Deleted local file: " + file);

						PostUploadtoPiSignage(file, up);

						await Task.Delay(1000);

						Root_Files rf = null;
						int attempt = 0;
						while (attempt < 5)
						{
							if (rf == null || rf.data.dbdata == null)
							{
								await Task.Delay(1000);
								rf = GetInfoAboutFileonPiSignage(file);
							}
							attempt += 1;
						}
						if (rf == null || rf.data.dbdata == null)
						{
							Libmiroppb.Log("Not proccessing file... Probably 4K");
							await SendNotificationAsync("Not proccessing file... Probably 4K");
							break;
						}

						//we're using the first WORD to determine which playlist the file goes to
						//this file will effectively replace the current file in the playlist, keeping all zones same
						AddFileToPlaylistonPiSignage(CurrentPlaylist, file, Convert.ToInt32(rf.data.dbdata.duration));

						//add file to database
						using (MySqlConnection conn = Secrets.GetConnectionString())
						{
							conn.Execute($"INSERT INTO files VALUES('{file}', '{CurrentPlaylist}');");
						}
						Libmiroppb.Log("Added " + file + " to the database");

						DeployPlaylistToGroup(CurrentPlaylist, CurrentPlaylist);

						timerRefreshFiles.Interval = 60 * 60 * 1000;
					}
					else
					{
						//file doesn't exist for some reason...
						Libmiroppb.Log($"{file} doesn't exist. Will try to redownload next time...");
						timerRefreshFiles.Interval = 60 * 1000; //01.08.23 Try a minute later, not an hour later
					}
				}
			}
			foreach (string a in DBFiles)
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
			Libmiroppb.Log("Deleted file: " + a + ", Response: " + files);

			//and database
			using (MySqlConnection conn = Secrets.GetConnectionString())
			{
				conn.Execute($"DELETE FROM files WHERE filename = '{a}'");
			}
			Libmiroppb.Log("Deleted file " + a + " from database");
		}

		private void AddFileToPlaylistonPiSignage(string pl, string file, int duration)
		{
			RefreshPlaylists();
			RefreshLayouts();

			Layouts CurrentLayout = AllLayouts.FirstOrDefault(x => x.Playlist == pl);

			//get current playlist
			Pi_Playlists.Datum Selected = AllPlaylists.First(x => x.name == pl);
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
			Libmiroppb.Log("Added to playlist " + pl + ": " + file + ", Response: " + playlistResponse);
		}

		private Root_Files GetInfoAboutFileonPiSignage(string file)
		{
			string json = SendRequest("/files/" + file, Method.Get, null);
			Libmiroppb.Log("Getting: " + file + ", Response: " + json);
			Root_Files rf = JsonConvert.DeserializeObject<Root_Files>(json);
			return rf;
		}

		private void PostUploadtoPiSignage(string file, string up)
		{
			Root_Files_Upload rfu = JsonConvert.DeserializeObject<Root_Files_Upload>(up);
			//process upload
			string post = SendRequest("/postupload", Method.Post, new { files = rfu.data });
			Libmiroppb.Log("PostUpload: " + file + ", Response: " + post);
		}

		private string UploadToPiSignage(string file)
		{
			string up = "";
			while (up == "" || up == null) //1.20.22 In case uploading fails (for unknown reason)
			{
				up = Upload("/files", file);
				Libmiroppb.Log("Upload attempt: " + file + ", Response: " + up);
				_ = SendNotificationAsync("Upload attempt " + file);
			}
			Libmiroppb.Log("Uploaded: " + file + ", Response: " + up);
			_ = SendNotificationAsync("Uploaded " + file);
			return up;
		}

		private string Upload(string url, string filename)
		{
			RestClient restClient = new(Settings.Api);
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
			RestClient restClient = new(Settings.Api);
			RestRequest restRequest;
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

		private static List<string> GetListOfFiles(string path) => Directory.GetFiles(path, "*.mp4", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToList(); //dont remove

		private void CheckNowToolStripMenuItem_Click(object sender, EventArgs e) => RefreshFiles();

		private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			notifyIcon1.Dispose();
			Application.Exit();
		}

		private void ScheduleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FrmSchedule frm = new()
			{
				ValidPlayers = Players.Select(x => x.Name).ToList(),
				ValidActions = GetEnumList<ScheduleActions>(),
				ValidPlaylists = AllPlaylists.Select(x => x.name).ToList()
			};
			frm.ShowDialog(this);

			RefreshSchedules();
		}

		private async void TimerSchedule_Tick()
		{
			//should be easy peasy right?
			foreach (ClSchedule a in Schedules)
			{
				if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.Day) && DateTime.Now.ToShortTimeString() == a.Time && Enum.Parse<ScheduleActions>(a.Action) == ScheduleActions.TurnOffTV)
				{
					Libmiroppb.Log($"Turning off TV {a.Player.Name}");
					await SendNotificationAsync($"Turning off TV {a.Player.Name}");
					SendRequest("/pitv/" + a.Player.Hex, Method.Post, new { status = true }); //true is off
					await Wait();
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.Day) && DateTime.Now.ToShortTimeString() == a.Time && Enum.Parse<ScheduleActions>(a.Action) == ScheduleActions.TurnOnTV)
				{
					bool? t = CheckOnlineStatus(a.Player.Name);
					if (t != null && t.Value)
					{
						Libmiroppb.Log($"Turning On TV {a.Player.Name}");
						await SendNotificationAsync($"Turning On TV {a.Player.Name}");
						SendRequest("/pitv/" + a.Player.Hex, Method.Post, new { status = false }); //false is on
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.Player.Name} is offline");
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.Day) && DateTime.Now.ToShortTimeString() == a.Time && Enum.Parse<ScheduleActions>(a.Action) == ScheduleActions.Reboot)
				{
					bool? t = CheckOnlineStatus(a.Player.Name);
					if (t != null && t.Value)
					{
						Libmiroppb.Log($"Rebooting TV {a.Player.Name}");
						await SendNotificationAsync($"Rebooting TV {a.Player.Name}");
						SendRequest("/pishell/" + a.Player.Hex, Method.Post, new { cmd = "shutdown -r now" });
						await Wait();
					}
					else
						await SendNotificationAsync($"TV {a.Player.Name} is offline");
				}
				else if (DateTime.Now.DayOfWeek == Enum.Parse<DayOfWeek>(a.Day) && DateTime.Now.ToShortTimeString() == a.Time && Enum.Parse<ScheduleActions>(a.Action) == ScheduleActions.DeployPlaylist)
				{
					bool? t = CheckOnlineStatus(a.Player.Name);
					if (t != null && t.Value && AllPlaylists.Any(x => x.name == a.Subaction))
					{
						Libmiroppb.Log($"Deploying: {a.Subaction} to {a.Name}");
						await SendNotificationAsync($"Deploying: {a.Subaction} to {a.Name}");
						DeployPlaylistToGroup(a.Subaction, a.Name);
						await Wait();
					}
				}
			}
		}

		private static async Task Wait(int s = 10)
		{
			Libmiroppb.Log($"Waiting {s} seconds..."); //02.11.22 Wait between requests
			await Task.Delay(1000 * s);
		}

		private void ShowPlayerIDsToolStripMenuItem_Click(object sender, EventArgs e)
		{
			FrmPlayers frm = new();

			foreach (ClPlayer p in Players)
			{
				frm.DgvPlayers.Rows.Add(p.Name, p.Hex);
				Libmiroppb.Log("Showing player name: " + p.Name + ", id:" + p.Hex);
			}
			frm.ShowDialog();
		}

		public async void OnToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Libmiroppb.Log("Turning all On");
			foreach (ClPlayer tv in Players)
			{
				await Wait();
				Libmiroppb.Log("Turning On Tv: " + tv.Name);
				SendRequest("/pitv/" + tv.Hex, Method.Post, new { status = false }); //false is on
			}
		}

		public async void OffToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Libmiroppb.Log("Turning all Off");
			foreach (ClPlayer tv in Players)
			{
				await Wait();
				Libmiroppb.Log("Turning Off Tv: " + tv.Name);
				SendRequest("/pitv/" + tv.Hex, Method.Post, new { status = true }); //false is on
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
			Pi_Playlists.Datum SelectedPlaylist = AllPlaylists.First(x => x.name == p);
			List<Pi_Playlists.Asset> assets = SelectedPlaylist.assets;
			List<string> col = [];
			foreach (Pi_Playlists.Asset a in assets)
			{
				col.AddRange([a.filename, a.bottom, a.side, a.zone4, a.zone5]);
			}
			col.AddRange([SelectedPlaylist.templateName, $"__{SelectedPlaylist.name}.json"]);
			ClDeployOptions options = new()
			{
				Assets = [.. col],
				Deploy = true
			};

			string group = SendRequest("/groups/" + Groups.Where(x => x.name == g).First()._id, Method.Post, options);
			Libmiroppb.Log($"Deployed {g}, with options: {options}, Response: {group}");
		}

		private void RebootPlayer(object sender, EventArgs e)
		{
			ToolStripItem i = sender as ToolStripItem;
			RebootPlayer(i.Text);
		}

		public void RebootPlayer(string groupName)
		{
			Libmiroppb.Log("Rebooting TV: " + Players.Where(x => x.Name == groupName).First().Name);
			SendRequest("/pishell/" + Players.Where(x => x.Name == groupName).First().Hex, Method.Post, new { cmd = "shutdown -r now" });
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
				{ "apikey", Settings.Prowl },
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
					status.Add(new() { Name = o.name, Status = new() { IsOnline = o.isConnected, CecStatus = o.cecTvStatus } });

				return status;
			}
			catch
			{
				return null;
			}
		}

		public bool? CheckOnlineStatus(string name)
		{
			string hex = Players.Where(x => x.Name == name).FirstOrDefault().Hex;
			if (hex != null)
				return CheckOnlineTVStatus().Where(x => x.Name == name).FirstOrDefault().Status.IsOnline;
			else
				return null;
		}

		public List<ClTVStatus> CheckAllDevicesOnlineStatus() => CheckOnlineTVStatus();

		public static List<string> GetEnumList<T>() where T : Enum => Enum.GetNames(typeof(T)).ToList();
	}
}
