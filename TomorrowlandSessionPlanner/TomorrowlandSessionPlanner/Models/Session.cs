namespace TomorrowlandSessionPlanner.Models;

public sealed record Session
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public int DjId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Stage? Stage { get; private set; }
    public Dj? Dj { get; private set; }
}