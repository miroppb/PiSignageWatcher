using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using miroppb;
using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Windows.Forms;

namespace PiSignageWatcher
{
	[SupportedOSPlatform("windows")]
	internal static class Program
	{
		public static FrmMain frm { get; private set; }

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			try
			{
				CreateWebHostBuilder(args).Build().RunAsync();

				Application.EnableVisualStyles();
				Application.SetHighDpiMode(HighDpiMode.SystemAware);
				Application.SetCompatibleTextRenderingDefault(false);
				frm = new FrmMain();
				Application.Run();
			}
			catch (Exception ex)
			{
				// Get stack trace for the exception with source file information
				var st = new StackTrace(ex, true);
				// Get the top stack frame
				var frame = st.GetFrame(0);
				// Get the line number from the stack frame
				var line = frame?.GetFileLineNumber();

				Libmiroppb.Log($"Error: {ex.Message} LINE: {line}");
			}
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			 WebHost.CreateDefaultBuilder(args).UseUrls("http://*:1112")
				 .UseStartup<Startup>();
	}
}
