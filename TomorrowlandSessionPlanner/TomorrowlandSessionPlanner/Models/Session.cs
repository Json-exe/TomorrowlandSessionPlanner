namespace TomorrowlandSessionPlanner.Models;

public sealed record Session
{
    public int Id { get; init; }
    public int StageId { get; init; }
    public int DjId { get; set; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
}