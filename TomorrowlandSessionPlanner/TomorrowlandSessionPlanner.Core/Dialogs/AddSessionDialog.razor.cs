using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Dialogs;

public partial class AddSessionDialog : ComponentBase
{
    [CascadingParameter]
    public required MudDialogInstance MudDialog { get; set; }
    
    [Parameter] public List<Dj> Djs { get; set; } = [];

    [Parameter] public List<Stage> Stages { get; set; } = [];
    
    [Parameter]
    public int PreSelectedStage { get; set; } = -1;
    
    [Parameter]
    public DateTime? PreSelectedDate { get; set; } = DateTime.Now;

    private int _selectedDj;
    private int _selectedStage;
    private DateTime? _startDate;
    private DateTime? _endDate;
    private TimeSpan? _startTime;
    private TimeSpan? _endTime;

    protected override void OnAfterRender(bool firstRender)
    {
        if (!firstRender) return;
        _selectedStage = PreSelectedStage >= 0 ? PreSelectedStage : Stages[0].Id;
        _startDate = PreSelectedDate;
        _endDate = PreSelectedDate;
        StateHasChanged();
    }

    private void Submit()
    {
        if (_endDate == null || _startDate == null || _startTime == null || _endTime == null || _selectedDj == 0 
            || _selectedStage == 0 || _startDate > _endDate)
        {
            Snackbar.Add("Please fill in all fields correctly", Severity.Error);
            return;
        }
        
        var session = new Session
        {
            DjId = _selectedDj,
            StageId = _selectedStage,
            StartTime = _startDate.Value.Date + _startTime.Value,
            EndTime = _endDate.Value.Date + _endTime.Value
        };
        MudDialog.Close(DialogResult.Ok(session));
    }

    private void Cancel() => MudDialog.Cancel();

    private Task<IEnumerable<string>> DjSearch(string arg)
    {
        return Task.FromResult(string.IsNullOrEmpty(arg) ? Djs.Select(x => x.Name) : Djs.Where(x => x.Name.Contains(arg, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Name));
    }

    private void DjChanged(string obj)
    {
        if (string.IsNullOrEmpty(obj)) return;
        _selectedDj = Djs.First(x => x.Name == obj).Id;
    }
}