using Dapper.Contrib.Extensions;
using PiSignageWatcher.JSON;
using System;

public enum ScheduleActions { TurnOffTV, TurnOnTV, Reboot, DeployPlaylist };

class ClSettings
{
    public int id { get; set; }
    public string user { get; set; }
	public string pass { get; set; }
	public string api { get; set; }
	public string prowl { get; set; }
    public string path { get; set; }
}

class ClFiles
{
	public string filename { get; set; }
	public string playlist { get; set; }
}

[Table("schedule")]
class ClSchedule
{
	public int id { get; set; }
	public string name { get; set; }
	[Computed]
	public ClPlayer player { get; set; }
	public string day { get; set; }
	public string time { get; set; }
	public string action { get; set; }
	public string subaction { get; set; }
}

public class ClPlayer
{
	public string name { get; set; }
	public string hex { get; set; }
}

class ClDeployOptions
{
	public bool deploy { get; set; } = true;
	public string orientation { get; set; } = "landscape";
	public string resolution { get; set; } = "auto";
	public bool exportAssets { get; set; } = false;
	public string[] assets { get; set; }

	public override string ToString()
	{
		return $"deploy: {deploy}, orientation: {orientation}, resolution: {resolution}, exportAssets: {exportAssets}, assets: {String.Join(", ", assets)}";
	}
}

public class ClTVStatus
{
	public string Name { get; set; }
	public ClStatus Status { get; set; }
}

public class ClStatus
{
	public bool IsOnline { get; set; }
	public bool cecStatus { get; set; }
}