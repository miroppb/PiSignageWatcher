using Dapper.Contrib.Extensions;
using System;

public enum ScheduleActions { TurnOffTV, TurnOnTV, Reboot, DeployPlaylist };

class ClSettings
{
	public int Id { get; set; }
	public string User { get; set; }
	public string Pass { get; set; }
	public string Api { get; set; }
	public string Prowl { get; set; }
	public string Path { get; set; }
    public string Otp { get; set; }
}

class ClFiles
{
	public string Filename { get; set; }
	public string Playlist { get; set; }
}

[Table("schedule")]
class ClSchedule
{
	public int Id { get; set; }
	public string Name { get; set; }
	[Computed]
	public ClPlayer Player { get; set; }
	public string Day { get; set; }
	public string Time { get; set; }
	public string Action { get; set; }
	public string Subaction { get; set; }
}

public class ClPlayer
{
	public string Name { get; set; }
	public string Hex { get; set; }
}

class ClDeployOptions
{
	public bool Deploy { get; set; } = true;
	public string Orientation { get; set; } = "landscape";
	public string Resolution { get; set; } = "auto";
	public bool ExportAssets { get; set; } = false;
	public string[] Assets { get; set; }

	public override string ToString()
	{
		return $"deploy: {Deploy}, orientation: {Orientation}, resolution: {Resolution}, exportAssets: {ExportAssets}, assets: {string.Join(", ", Assets)}";
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
	public bool CecStatus { get; set; }
}

class Layouts
{
    public int Id { get; set; }
    public string Playlist { get; set; }
    public bool Fullscreen { get; set; }
    public string Side { get; set; }
    public string Bottom { get; set; }
    public string Zone4 { get; set; }
    public string Zone5 { get; set; }
}