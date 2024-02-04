using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Dialogs;

public partial class SupplementSessionsDialog : ComponentBase
{
    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public List<Session> SupplementSessions { get; set; } = [];

    private readonly DateTime _weekend2Start = new(2023, 7, 28);
    private readonly DateTime _weekend1Start = new(2023, 7, 21);

    private void AddSessionToUser(Session session)
    {
        PlannerManager.AddedSessions.Add(session);
        SupplementSessions = PlannerManager.SessionList.Where(s => !PlannerManager.AddedSessions.Any(ss => IsSessionOverlapping(s, ss) && PlannerManager.AddedSessions.Any(session1 => session1.Id != s.Id))).ToList();
        StateHasChanged();
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }
}