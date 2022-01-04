using System;

class Stuff
{
    public enum ScheduleActions { TurnOffTV, TurnOnTV };

    public string Tv { get; set; }
    public DayOfWeek DoW { get; set; }
    public DateTime Dt { get; set; }
    public ScheduleActions Sa { get; set; }
}