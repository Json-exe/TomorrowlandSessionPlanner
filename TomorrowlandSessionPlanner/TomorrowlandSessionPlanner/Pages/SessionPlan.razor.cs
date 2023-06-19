using MudBlazor;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages;

public partial class SessionPlan
{
    private bool _loading = true;

    private readonly List<OverlappingSessionClass> _overlappingSessionClasses = new();

    private async Task CheckSessions()
    {
        _loading = true;
        _overlappingSessionClasses.Clear();
        StateHasChanged();
        var allOverlappingSessions = new List<Session>();
        var overlappingSessions = new List<OverlappingSessionClass>();
        foreach (var addedSession in PlannerManager.AddedSessions)
        {
            var isBetween = false;
            var overlapSession = new OverlappingSessionClass
            {
                Session = addedSession,
                OverlappingSessions = new List<Session>()
            };

            foreach (var otherSession in PlannerManager.AddedSessions.Where(x => x != addedSession))
            {
                if (!IsSessionOverlapping(addedSession, otherSession)) continue;
                isBetween = true;
                overlapSession.OverlappingSessions.Add(otherSession);
            }

            if (isBetween)
            {
                allOverlappingSessions.Add(addedSession);
                overlappingSessions.Add(overlapSession);
            }
        }

        if (allOverlappingSessions.Count > 1)
        {
            _overlappingSessionClasses.AddRange(overlappingSessions);
            Snackbar.Add(
                $"Du hast {allOverlappingSessions.Count} überschneidende Sessions in deinem Plan! Bitte überprüfen deinen Plan erneut.",
                Severity.Info);
        }
        else
        {
            Snackbar.Add($"Deine Session Liste sieht gut aus! Überspringe zum Ergebniss!", Severity.Success);
            await Task.Delay(1500);
            NavigationManager.NavigateTo("/Result");
            return;
        }

        _loading = false;
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CheckSessions();
        }
    }

    private bool IsSessionOverlapping(Session session1, Session session2)
    {
        // Überprüfen, ob das Ende von session1 nach dem Start von session2 liegt und
        // das Start von session1 vor dem Ende von session2 liegt
        if (session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime)
        {
            return true;
        }

        return false;
    }

    // private bool IsSessionBetween(Session session, Session otherSession)
    // {
    //     // Definieren des Zeitpuffers von 5 Minuten
    //     var buffer = TimeSpan.FromMinutes(5);
    //
    //     // Überprüfen, ob das Start- und Enddatum der Session innerhalb des Zeitbereichs der anderen Session liegt
    //     if (session.StartTime > otherSession.StartTime.Add(-buffer) && session.EndTime < otherSession.EndTime.Add(buffer))
    //     {
    //         return true;
    //     }
    //
    //     return false;
    // }

    private async void RemoveUserSession(Session session)
    {
        PlannerManager.AddedSessions.Remove(session);
        Snackbar.Add("Session aus deinem Plan entfernt", Severity.Success);
        await CheckSessions();
    }
}