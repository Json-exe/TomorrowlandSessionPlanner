namespace TomorrowlandSessionPlanner.Core.Model;

public sealed record Lobby
{
    public Guid LobbyId { get; } = Guid.NewGuid();
    public List<string> ConnectedUsers { get; set; } = [];
}