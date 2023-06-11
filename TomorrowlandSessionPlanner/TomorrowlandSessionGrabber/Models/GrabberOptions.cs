namespace TomorrowlandSessionGrabber.Models;

public class GrabberOptions
{
    public bool Headless { get; set; }
    public Weekend Weekend { get; set; } = Weekend.Weekend1;
}

public enum Weekend
{
    Weekend1,
    Weekend2
}