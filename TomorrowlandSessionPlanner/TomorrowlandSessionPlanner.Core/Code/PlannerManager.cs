using Microsoft.EntityFrameworkCore;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.DBContext;
using TomorrowlandSessionPlanner.Core.Model;
using TomorrowlandSessionPlanner.Core.Services;

namespace TomorrowlandSessionPlanner.Core.Code;

public class PlannerManager : IAsyncDisposable
{
    private IDbContextFactory<TmldbContext> DbContextFactory { get; }
    private LobbyService LobbyService { get; }
    private ISnackbar Snackbar { get; }
    public readonly List<Session> AddedSessions = [];
    public readonly List<Dj> DjList = [];
    public readonly List<Stage> StageList = [];
    public readonly List<Session> SessionList = [];

    public PlannerManager(IDbContextFactory<TmldbContext> dbContextFactory, LobbyService lobbyService, ISnackbar snackbar)
    {
        DbContextFactory = dbContextFactory;
        LobbyService = lobbyService;
        Snackbar = snackbar;
        LobbyService.LobbyChangedEvent += LobbyChanged;
    }

    private void LobbyChanged(object? sender, EventArgs args)
    {
        Snackbar.Add("Lobby has changed!");
    }

    public async Task Init()
    {
        if (DjList.Count != 0 && StageList.Count != 0 && SessionList.Count != 0) return;

        var dbContext = await DbContextFactory.CreateDbContextAsync();
        DjList.AddRange(await dbContext.Djs.ToListAsync());
        StageList.AddRange(await dbContext.Stages.ToListAsync());
        SessionList.AddRange(await dbContext.Sessions.ToListAsync());
    }

    public async ValueTask DisposeAsync()
    {
        LobbyService.LobbyChangedEvent -= LobbyChanged;
        await LobbyService.DisposeAsync();
    }
}