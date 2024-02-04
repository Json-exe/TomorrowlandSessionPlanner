namespace TomorrowlandSessionGrabber.Models;

public class GrabberOptions
{
    public bool Headless { get; init; }
    public Weekend Weekend { get; init; } = Weekend.Weekend1;
}

public enum Weekend
{
    Weekend1,
    Weekend2
}