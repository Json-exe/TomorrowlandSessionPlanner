using Microsoft.EntityFrameworkCore;
using TomorrowlandSessionPlanner.Core.DBContext;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Code;

public class PlannerManager
{
    private IDbContextFactory<TmldbContext> DbContextFactory { get; set; }
    public readonly List<Session> AddedSessions = new();
    public readonly List<Dj> DjList = new();
    public readonly List<Stage> StageList = new();
    public readonly List<Session> SessionList = new();

    public PlannerManager(IDbContextFactory<TmldbContext> dbContextFactory)
    {
        DbContextFactory = dbContextFactory;
    }
    
    public async Task Init()
    {
        if (DjList.Any() && StageList.Any() && SessionList.Any()) return;

        var dbContext = await DbContextFactory.CreateDbContextAsync();
        DjList.AddRange(dbContext.Djs);
        StageList.AddRange(dbContext.Stages);
        SessionList.AddRange(dbContext.Sessions);
    }
}