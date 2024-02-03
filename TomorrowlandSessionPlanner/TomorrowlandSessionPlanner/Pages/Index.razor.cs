using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Index : ComponentBase
{
    private readonly List<Dj> _djList = new();
    private readonly List<Stage> _stageList = new();
    private readonly List<Session> _sessionList = new();
    private List<Session> _filteredSessions = new();
    private readonly DateTime _weekend2Start = new(2023, 7, 28);
    private readonly DateTime _weekend1Start = new(2023, 7, 21);
    private IEnumerable<Stage> _stageFilter = new List<Stage>();
    private string? _djFilter;
    private bool _loading = true;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await PlannerManager.Init();
            _sessionList.AddRange(PlannerManager.SessionList);
            _djList.AddRange(PlannerManager.DjList);
            _stageList.AddRange(PlannerManager.StageList);
            _filteredSessions.AddRange(_sessionList);
            _loading = false;
            StateHasChanged();
        }
    }
    
    private Task<IEnumerable<string>> DjSearch(string arg)
    {
        return Task.FromResult(string.IsNullOrEmpty(arg) ? _djList.Select(x => x.Name) : 
            _djList.Where(x => x.Name.Contains(arg, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Name));
    }
    
    private void ApplyFilters()
    {
        if (!_stageFilter.Any() && string.IsNullOrEmpty(_djFilter))
        {
            _filteredSessions = _sessionList;
            StateHasChanged();
            return;
        }
        var dj = _djList.FirstOrDefault(x => _djFilter != null && x.Name.Equals(_djFilter, StringComparison.InvariantCultureIgnoreCase));
        // Filter by stage and dj
        if (_stageFilter.Any() && !string.IsNullOrEmpty(_djFilter))
        {
            if (dj == null) return;
            _filteredSessions = _sessionList.Where(x => _stageFilter.Any(y => y.Id == x.StageId) && x.DjId == dj.Id).ToList();
            StateHasChanged();
            return;
        }
        // Filter by stage
        if (_stageFilter.Any())
        {
            _filteredSessions = _sessionList.Where(x => _stageFilter.Any(y => y.Id == x.StageId)).ToList();
            StateHasChanged();
            return;
        }
        // Filter by dj
        if (dj == null) return;
        _filteredSessions = _sessionList.Where(x => _djFilter != null && x.DjId == dj.Id).ToList();
        StateHasChanged();
    }
    
    private void NavigateToSummary()
    {
        if (!PlannerManager.AddedSessions.Any())
        {
            Snackbar.Add("Bitte wähle mindestens eine Session aus!", Severity.Info);
            return;
        }
        NavigationManager.NavigateTo("/summary");
    }

    private static string ToStringFunc(Stage arg)
    {
        return arg.Name;
    }
}