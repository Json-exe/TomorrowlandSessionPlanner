using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public partial class SessionAnalyzerViewModel : Model.ViewModel
{
    private readonly PlannerManager _plannerManager;
    private readonly ISnackbar _snackbar;
    private readonly NavigationManager _navigationManager;

    [ObservableProperty] private List<OverlappingSession> _overlappingSessions = [];
    [ObservableProperty] private bool _sessionsHaveChanged;

    public SessionAnalyzerViewModel(PlannerManager plannerManager, ISnackbar snackbar, NavigationManager navigationManager)
    {
        _plannerManager = plannerManager;
        _snackbar = snackbar;
        _navigationManager = navigationManager;
    }

    /// <summary>
    /// Checks if there are any sessions that overlap other sessions.
    /// </summary>
    [RelayCommand]
    private async Task CheckSessions()
    {
        var allOverlappingSessions = new List<Session>();
        var overlappingSessions = new List<OverlappingSession>();
        foreach (var addedSession in _plannerManager.AddedSessions)
        {
            if (!IsBetween(addedSession, out var overlapSession)) continue;
            allOverlappingSessions.Add(addedSession);
            overlappingSessions.Add(overlapSession);
        }

        if (allOverlappingSessions.Count > 1)
        {
            OverlappingSessions = overlappingSessions;
            _snackbar.Add(
                $"Du hast {allOverlappingSessions.Count} überschneidende Sessions in deinem Plan! Bitte überprüfen deinen Plan erneut.",
                Severity.Info);
        }
        else
        {
            _snackbar.Add("Deine Session Liste sieht gut aus! Überspringe zum Ergebniss!", Severity.Success);
            await Task.Delay(1000);
            _navigationManager.NavigateTo("/Result");
        }
    }

    private bool IsBetween(Session addedSession, [NotNullWhen(true)] out OverlappingSession? overlapSession)
    {
        overlapSession = new OverlappingSession
        {
            Session = addedSession,
            OverlappingSessions = []
        };
        
        foreach (var otherSession in _plannerManager.AddedSessions.Where(x => x != addedSession))
        {
            if (!IsSessionOverlapping(addedSession, otherSession)) continue;
            overlapSession.OverlappingSessions.Add(otherSession);
        }

        return overlapSession.OverlappingSessions.Count > 0;
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        // Check if the EndTime of session1 is after the StartTime of session2 and
        // the StartTime of session1 is after the EndTime of session2
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }
}