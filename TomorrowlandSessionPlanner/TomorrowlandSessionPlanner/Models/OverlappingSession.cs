namespace TomorrowlandSessionPlanner.Models;

public sealed record OverlappingSession
{
    public Session Session { get; init; } = new();
    public List<Session> OverlappingSessions { get; init; } = new();
}