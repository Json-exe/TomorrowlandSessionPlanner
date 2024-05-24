using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public partial class ResultViewModel : Model.ViewModel
{
    [ObservableProperty] private IJSObjectReference? _objectReference;
    [ObservableProperty] private List<Session> _sortedSessions = [];

    private readonly IJSRuntime _jsRuntime;
    private readonly PlannerManager _plannerManager;
    private readonly NavigationManager _navigationManager;

    public ResultViewModel(IJSRuntime jsRuntime, PlannerManager plannerManager, NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _plannerManager = plannerManager;
        _navigationManager = navigationManager;
    }

    [RelayCommand]
    private async Task InitResultPage(CancellationToken cancellationToken)
    {
        if (_plannerManager.AddedSessions.Count == 0)
        {
            _navigationManager.NavigateTo("/", true, true);
            return;
        }

        ObjectReference =
            await _jsRuntime.InvokeAsync<IJSObjectReference>("import", cancellationToken, "./Pages/Result.razor.js");
        
        SortedSessions = _plannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
    }
}