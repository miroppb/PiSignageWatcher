using System;

public enum ScheduleActions { TurnOffTV, TurnOnTV, Reboot };

class ClSettings
{
    public string user { get; set; }
    public string pass { get; set; }
    public string api { get; set; }
    public string gdrive { get; set; }
    public string prowl { get; set; }
}

class ClFiles
{
    public string filename { get; set; }
    public string playlist { get; set; }
}

class ClGroups
{
    public string name { get; set; }
    public string hex { get; set; }
}

class ClPlaylists
{
    public string search { get; set; }
    public string name { get; set; }
}

class ClSchedule
{
    public int id { get; set; }
    public ClTV tv { get; set; }
    public DayOfWeek day { get; set; }
    public DateTime time { get; set; }
    public ScheduleActions action { get; set; }
}

class ClTV
{
    public string name { get; set; }
    public string hex { get; set; }
}