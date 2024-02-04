namespace TomorrowlandSessionGrabber.Models;

public class Session
{
    public string StageName { get; init; } = string.Empty;
    public string DjName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}