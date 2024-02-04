using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Model;
using TomorrowlandSessionPlanner.Dialogs;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Result : ComponentBase, IAsyncDisposable
{
    private bool _loading = true;
    private List<Session> _sortedSessions = [];
    private IJSObjectReference? _jsModule;
    private readonly DateTime _weekend2Start = new(2023, 7, 28);
    private readonly DateTime _weekend1Start = new(2023, 7, 21);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Result.razor.js");
        if (firstRender)
        {
            if (PlannerManager.AddedSessions.Count == 0)
            {
                NavigationManager.NavigateTo("/", true, true);
                return;
            }

            _sortedSessions = PlannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
            _loading = false;
            StateHasChanged();
        }
    }

    private async Task ShowSupplements()
    {
        var allSessions = PlannerManager.SessionList.Where(s => !PlannerManager.AddedSessions.Any(ss =>
            IsSessionOverlapping(s, ss) && PlannerManager.AddedSessions.Any(session => session.Id != s.Id))).ToList();
        var dialogParameters = new DialogParameters
        {
            { "supplementSessions", allSessions }
        };
        var dialogReference = await DialogService.ShowAsync<SupplementSessionsDialog>("Supplement Sessions", dialogParameters,
            new DialogOptions
            {
                CloseButton = false, MaxWidth = MaxWidth.ExtraExtraLarge, FullWidth = true, CloseOnEscapeKey = false,
                DisableBackdropClick = true
            });
        await dialogReference.Result;
        _sortedSessions = PlannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
        StateHasChanged();
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        // Überprüfen, ob das Ende von session1 nach dem Start von session2 liegt und
        // das Start von session1 vor dem Ende von session2 liegt
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }
    
    /// <summary>
    /// Downloads a file by serializing a sorted list of sessions as JSON and saving it to a file.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task DownloadFile()
    {
        var json = JsonSerializer.Serialize(_sortedSessions);
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.tmlplanner");
        await File.WriteAllTextAsync(savePath, json);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (_jsModule != null) 
            await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async Task DownloadHtmlFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.html");
        var html = await new HtmlCreator().CreateHtmlTable(_sortedSessions, PlannerManager);
        await File.WriteAllTextAsync(savePath, html);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (_jsModule != null) 
            await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null) await _jsModule.DisposeAsync();
    }
}