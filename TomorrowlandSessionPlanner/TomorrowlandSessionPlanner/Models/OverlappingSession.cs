namespace TomorrowlandSessionPlanner.Models;

public class OverlappingSession
{
    public Session Session { get; set; } = new();
    public List<Session> OverlappingSessions { get; set; } = new();
}