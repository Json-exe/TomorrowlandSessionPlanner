using Microsoft.AspNetCore.Components;
using TomorrowlandSessionPlanner.Code;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Components;

public partial class SessionCard : ComponentBase
{
    [Parameter, EditorRequired] public required Session Session { get; set; }
    [Parameter] public bool IsReadonly { get; set; }
    [Parameter] public string Class { get; set; } = string.Empty;
    [Inject] public required PlannerManager PlannerManager { get; set; }
    
    private void AddUserSession(Session session)
    {
        PlannerManager.AddedSessions.Add(session);
    }
    
    private void RemoveUserSession(Session session)
    {
        PlannerManager.AddedSessions.Remove(session);
    }
}