using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PiSignageWatcher.Controllers
{
	[EnableCors("Policy")]
	[Route("api")]
	[ApiController]
	public class PSController : ControllerBase
	{
		[HttpGet]
		public ContentResult Get()
		{
			var html = System.IO.File.ReadAllText(@"api.html");
			return base.Content(html, "text/html");
		}

		[HttpGet("{endpoint}")]
		public ActionResult<string> Get(string endpoint)
		{
			object ret = null;
			switch (endpoint)
			{
				case "history":
					StreamReader r = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PiSignageWatcher\\log.log");
					List<string> lst = r.ReadToEnd().Split(Environment.NewLine).ToList();
					r.Close();
					ret = lst.TakeLast(50).ToList();
					break;
				case "checknow":
					Program.frm.RefreshFiles();
					break;
				case "getdevices":
					if (Program.frm != null)
					{
						Program.frm.PopulateData();
						List<object> temp = new();
						foreach (ClPlayer p in Program.frm.Players)
						{
							temp.Add(new { p.Name });
						}
						ret = temp;
					}
					break;
				case "status":
					if (Program.frm != null)
						ret = Program.frm!.CheckAllDevicesOnlineStatus();
					break;
			}
			return Ok(ret);
		}

		[HttpGet("{_action}/{device}")]
		public ActionResult<string> Get(string _action, string device)
		{
			object ret = null;
			switch (_action)
			{
				case "redeploy":
					if (Program.frm != null)
					{
						if (Program.frm.Players.Any(x => x.Name.StartsWith(device)))
						{
							Program.frm.DeployPlaylistToGroup(device, device);
							ret = new { message = $"Redeploy of {device} Successful" };
						}
						else
							ret = new { message = $"Device Name: {device} doesn't exist" };
					}
					break;
				case "powerall":
					switch (device)
					{
						case "off":
							Program.frm.offToolStripMenuItem_Click(null, null);
							ret = new { message = "Power Off sent" };
							break;
						case "on":
							Program.frm.onToolStripMenuItem_Click(null, null);
							ret = new { message = "Power On sent" };
							break;
						default:
							ret = new { message = "You can only turn devices off or on" };
							break;
					}
					break;
				case "reboot":
					if (Program.frm.Players.FirstOrDefault(x => x.Name == device) != null)
					{
						Program.frm.RebootPlayer(device);
						ret = new { message = $"Reboot of {device} Successful" };
					}
					else
						ret = new { message = $"Device Name: {device} doesn't exist" };
					break;
			}
			return Ok(ret);
		}
	}

	[EnableCors("Policy")]
	[Route("")]
	[ApiController]
	public class RootController : ControllerBase
	{
		[HttpGet]
		public static IEnumerable<string> Get()
		{
			return new string[] { "Scary music comes from PiSignage" };
		}

		[HttpGet("{id}")]
		public ActionResult GetFile(string id)
		{
			if (id == "favicon.ico")
			{
				return new FileStreamResult(new FileStream("signage.ico", FileMode.Open), "image/x-icon");
			}
			else
				return Forbid();
		}
	}
}
