namespace TomorrowlandSessionPlanner.Models;

public sealed record Stage
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
}