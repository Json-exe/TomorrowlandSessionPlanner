using System.Text.Json.Serialization;

namespace TomorrowlandSessionPlanner.Core.Model;

public sealed record Session
{
    public int Id { get; set; }
    public int StageId { get; set; }
    public int DjId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    [JsonIgnore] public Stage? Stage { get; private set; }
    [JsonIgnore] public Dj? Dj { get; private set; }
}