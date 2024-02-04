using Microsoft.AspNetCore.SignalR;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Hub;


public class LobbyHub : Microsoft.AspNetCore.SignalR.Hub
{
    public LobbyHub(ILogger<LobbyHub> logger)
    {
        Logger = logger;
    }

    private ILogger<LobbyHub> Logger { get; }
    private List<Lobby> Lobbies { get; } = [];

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        Logger.LogInformation("Disconnected client: {Id}", Context.ConnectionId);
        if (!Lobbies.Any(x => x.ConnectedUsers.Any(c => c == Context.ConnectionId)))
            return base.OnDisconnectedAsync(exception);

        var lobbies = Lobbies.Where(x => x.ConnectedUsers.Any(x => x == Context.ConnectionId));
        foreach (var lobby in lobbies)
        {
            lobby.ConnectedUsers.Remove(Context.ConnectionId);
        }

        Lobbies.RemoveAll(x => x.ConnectedUsers.Count == 0);
        return base.OnDisconnectedAsync(exception);
    }

    public override Task OnConnectedAsync()
    {
        Logger.LogInformation("Client connected: {Id}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public async Task<Guid> JoinOrCreateLobby(bool newLobby, string lobbyId = "")
    {
        Logger.LogInformation("Joining/Creating lobby with args: {New}, {LobbyId}", newLobby, lobbyId);
        if (newLobby)
        {
            var lobby = new Lobby
            {
                ConnectedUsers = [Context.ConnectionId]
            };
            Lobbies.Add(lobby);
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId.ToString());
            return lobby.LobbyId;
        }

        if (!string.IsNullOrEmpty(lobbyId) && Lobbies.Any(x => x.LobbyId.ToString() == lobbyId))
        {
            var lobby = Lobbies.First(x => x.LobbyId.ToString() == lobbyId);
            lobby.ConnectedUsers.Add(Context.ConnectionId);
            await Clients.Group(lobby.LobbyId.ToString()).SendAsync("LobbyChanged");
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby.LobbyId.ToString());
            return lobby.LobbyId;
        }

        Context.Abort();
        return Guid.Empty;
    }
}