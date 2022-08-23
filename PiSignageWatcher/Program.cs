using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PiSignageWatcher
{
    internal static class Program
    {
        public static FrmMain frm { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().RunAsync();

            Application.EnableVisualStyles();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            frm = new FrmMain();
            Application.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
             WebHost.CreateDefaultBuilder(args).UseUrls("http://*:1112")
                 .UseStartup<Startup>();
    }
}
