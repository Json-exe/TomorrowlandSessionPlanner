using Microsoft.AspNetCore.SignalR.Client;

namespace TomorrowlandSessionPlanner.Core.Services;

public class LobbyService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private Guid ConnectedLobbyId { get; set; }

    public EventHandler? LobbyChangedEvent;

    private async Task Connect()
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:44354/hubs/lobby")
            .WithAutomaticReconnect()
            .Build();

        await _hubConnection.StartAsync();
    }

    public async Task CreateLobby()
    {
        if (_hubConnection is not { State: HubConnectionState.Connected })
        {
            await Connect();
        }
        else if (ConnectedLobbyId != Guid.Empty)
        {
            await _hubConnection.StopAsync();
            await Connect();
        }
        
        ConnectedLobbyId = await _hubConnection!.InvokeAsync<Guid>("JoinOrCreateLobby", true, "");
        _hubConnection?.On("LobbyChanged", () =>
        {
            Console.WriteLine("Hello from LobbyChanged CreateLobby");
            LobbyChangedEvent?.Invoke(this, EventArgs.Empty);
        });
        Console.WriteLine($"Connected to lobby: {ConnectedLobbyId}");
    }

    public async Task JoinLobby(string id)
    {
        if (_hubConnection is not { State: HubConnectionState.Connected })
        {
            await Connect();
        }
        else if (ConnectedLobbyId != Guid.Empty)
        {
            await _hubConnection.StopAsync();
            await Connect();
        }
        
        ConnectedLobbyId = await _hubConnection!.InvokeAsync<Guid>("JoinOrCreateLobby", false, id);
        if (ConnectedLobbyId == Guid.Empty)
        {
            return;  
        }
        _hubConnection?.On("LobbyChanged", () =>
        {
            Console.WriteLine("Hello from LobbyChanged JoinLobby");
            LobbyChangedEvent?.Invoke(this, EventArgs.Empty);
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            if (_hubConnection.State is HubConnectionState.Connected) await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
        }
    }
}