using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Code;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Components;

public partial class SessionAnalyzeDetailCard : ComponentBase
{
    [Parameter, EditorRequired] public required OverlappingSession OverlappingSession { get; set; }
    [Inject] public required PlannerManager PlannerManager { get; set; }
    [Inject] public required ISnackbar Snackbar { get; set; }
    [Parameter] public EventCallback SessionRemoved { get; set; }
    
    private async void RemoveUserSession(Session session)
    {
        PlannerManager.AddedSessions.Remove(session);
        Snackbar.Add("Session aus deinem Plan entfernt", Severity.Success);
        await SessionRemoved.InvokeAsync();
    }
}