namespace TomorrowlandSessionPlanner.Models;

public sealed record SessionImportModel
{
    public string StageName { get; set; } = string.Empty;
    public readonly string DjName = string.Empty;
    public DateTime StartTime = DateTime.Now;
    public DateTime EndTime = DateTime.Now;
}