using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SQLite;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace PiSignageWatcher.Controllers
{
    [EnableCors("Policy")]
    [Route("api")]
    [ApiController]
    public class PSController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "Hello" };
        }

        [HttpGet("{endpoint}")]
        public ActionResult<string> Get(string endpoint)
        {
            object ret = null;
            switch (endpoint)
            {
                case "history":
                    StreamReader r = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\PiSignageWatcher\\log.log");
                    List<string> lst= r.ReadToEnd().Split(Environment.NewLine).ToList();
                    r.Close();
                    ret = lst.TakeLast(50).ToList();
                    break;
                case "checknow":
                    Program.frm.timerRefresh_Tick(null, null);
                    break;
                case "getdevices":
                    using (SQLiteConnection conn = GetSQLConnection())
                        ret = conn.Query<ClGroups>("SELECT name FROM groups").ToList();
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
                    using (SQLiteConnection conn = GetSQLConnection())
                    {
                        if (conn.Query<ClGroups>("SELECT * FROM groups WHERE name = @device", new DynamicParameters(new { device = device })).FirstOrDefault() != null)
                        {
                            Program.frm.reDeployGroup(device);
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
                    using (SQLiteConnection conn = GetSQLConnection())
                    {
                        if (conn.Query<ClGroups>("SELECT * FROM groups WHERE name = @device", new DynamicParameters(new { device = device })).FirstOrDefault() != null)
                        {
                            Program.frm.rebootGroup(device);
                            ret = new { message = $"Reboot of {device} Successful" };
                        }
                        else
                            ret = new { message = $"Device Name: {device} doesn't exist" };
                    }
                    break;
            }
            return Ok(ret);
        }

        SQLiteConnection GetSQLConnection()
        {
            return new SQLiteConnection("Data Source=" + Application.StartupPath + "\\db.db;Version=3;");
        }
    }
}
