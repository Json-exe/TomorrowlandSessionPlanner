using Microsoft.EntityFrameworkCore;
using TomorrowlandSessionPlanner.Core.DBContext;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Code;

public class PlannerManager
{
    private IDbContextFactory<TmldbContext> DbContextFactory { get; }
    public readonly List<Session> AddedSessions = [];
    public readonly List<Dj> DjList = [];
    public readonly List<Stage> StageList = [];
    public readonly List<Session> SessionList = [];

    public PlannerManager(IDbContextFactory<TmldbContext> dbContextFactory)
    {
        DbContextFactory = dbContextFactory;
    }
    
    public async Task Init()
    {
        if (DjList.Count != 0 && StageList.Count != 0 && SessionList.Count != 0) return;

        var dbContext = await DbContextFactory.CreateDbContextAsync();
        DjList.AddRange(await dbContext.Djs.ToListAsync());
        StageList.AddRange(await dbContext.Stages.ToListAsync());
        SessionList.AddRange(await dbContext.Sessions.ToListAsync());
    }
}