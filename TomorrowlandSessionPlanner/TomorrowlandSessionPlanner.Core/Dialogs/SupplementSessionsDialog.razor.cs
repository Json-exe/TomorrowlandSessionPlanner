using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Dialogs;

public partial class SupplementSessionsDialog : ComponentBase
{
    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }

    [Parameter]
    public List<Session> SupplementSessions { get; set; } = [];

    private void AddSessionToUser(Session session)
    {
        PlannerManager.AddedSessions.Add(session);
        SupplementSessions =
            PlannerManager.SessionList
                .Where(s => !PlannerManager.AddedSessions.Exists(ads => ads.Id == s.Id))
                .Where(s => !PlannerManager.AddedSessions.Exists(ads => IsSessionOverlapping(s, ads)))
                .ToList();
        StateHasChanged();
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }
}