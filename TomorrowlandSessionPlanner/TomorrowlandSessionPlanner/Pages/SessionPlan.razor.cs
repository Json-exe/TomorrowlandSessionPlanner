using TomorrowlandSessionPlanner.Components;
using TomorrowlandSessionPlanner.Core.ViewModel;

namespace TomorrowlandSessionPlanner.Pages;

public partial class SessionPlan : ViewModelComponent<SessionAnalyzerViewModel>
{
    protected override void OnInitialized()
    {
        if (PlannerManager.AddedSessions.Count == 0)
        {
            NavigationManager.NavigateTo("/", true, true);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        await ViewModel.CheckSessionsCommand.ExecuteAsync(null);
        StateHasChanged();
    }

    private void SessionRemoved()
    {
        ViewModel.SessionsHaveChanged = true;
    }
    
    private async Task ReCheckSessions()
    {
        await ViewModel.CheckSessionsCommand.ExecuteAsync(null);
    }
}