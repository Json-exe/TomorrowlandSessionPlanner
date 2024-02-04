using System.Text.Json.Serialization;

namespace TomorrowlandSessionPlanner.Core.Model;

public sealed record Session
{
    public int Id { get; init; }
    public int StageId { get; init; }
    public int DjId { get; set; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    [JsonIgnore] public Stage? Stage { get; private set; }
    [JsonIgnore] public Dj? Dj { get; private set; }
}