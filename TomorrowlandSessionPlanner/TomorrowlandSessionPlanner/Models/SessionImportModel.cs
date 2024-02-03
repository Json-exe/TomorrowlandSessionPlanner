namespace TomorrowlandSessionPlanner.Models;

public sealed record SessionImportModel
{
    public string StageName { get; set; } = string.Empty;
    public string DjName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}