using Aspose.Pdf;
using Aspose.Pdf.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Newtonsoft.Json;
using TomorrowlandSessionPlanner.Code;
using TomorrowlandSessionPlanner.Dialogs;
using TomorrowlandSessionPlanner.Models;
using Color = Aspose.Pdf.Color;
using HorizontalAlignment = Aspose.Pdf.HorizontalAlignment;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Result : ComponentBase, IAsyncDisposable
{
    private bool _loading = true;
    private List<Session> _sortedSessions = new();
    private IJSObjectReference? _jsModule;
    private readonly DateTime _weekend2Start = new(2023, 7, 28, 00, 0, 0);
    private readonly DateTime _weekend1Start = new(2023, 7, 21, 00, 0, 0);

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

    private async void ShowSupplements()
    {
        var allSessions = PlannerManager.SessionList.Where(s => !PlannerManager.AddedSessions.Any(ss =>
            IsSessionOverlapping(s, ss) && PlannerManager.AddedSessions.Any(session => session.id != s.id))).ToList();
        var dialogParameters = new DialogParameters
        {
            { "supplementSessions", allSessions }
        };
        await DialogService.ShowAsync<SupplementSessionsDialog>("Supplement Sessions", dialogParameters,
            new DialogOptions
            {
                CloseButton = false, MaxWidth = MaxWidth.ExtraExtraLarge, FullWidth = true, CloseOnEscapeKey = false,
                DisableBackdropClick = true
            });
        _sortedSessions = PlannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
        StateHasChanged();
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        // Überprüfen, ob das Ende von session1 nach dem Start von session2 liegt und
        // das Start von session1 vor dem Ende von session2 liegt
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }

    private async void DownloadFile()
    {
        var json = JsonConvert.SerializeObject(_sortedSessions, Formatting.Indented);
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.tmlplanner");
        await File.WriteAllTextAsync(savePath, json);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (_jsModule != null) 
            await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async void DownloadHtmlFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.html");
        var html = await new HTMLCreator().CreateHTMLTable(_sortedSessions, PlannerManager);
        await File.WriteAllTextAsync(savePath, html);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (_jsModule != null) 
            await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async void DownloadPdfFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.pdf");
        var document = new Document();
        var page = document.Pages.Add();
        var table = new Table
        {
            ColumnWidths = "100"
        };

        var headerRow = table.Rows.Add();
        headerRow.Cells.Add("Bühne");
        headerRow.Cells.Add("DJ");
        headerRow.Cells.Add("Start");
        headerRow.Cells.Add("Ende");
        foreach (var sortedSession in _sortedSessions)
        {
            var djName = PlannerManager.DjList.FirstOrDefault(d => d.id == sortedSession.DJId)?.Name;
            var stageName = PlannerManager.StageList.FirstOrDefault(s => s.id == sortedSession.StageId)?.Name;
            var row = table.Rows.Add();
            row.Cells.Add(stageName);
            row.Cells.Add(djName);
            row.Cells.Add(sortedSession.StartTime.ToString("dd-MM-yyyy HH:mm"));
            row.Cells.Add(sortedSession.EndTime.ToString("dd-MM-yyyy HH:mm"));
        }

        // Create a title text for the PDF
        var title = new TextFragment("Tomorrowland Session Planner")
        {
            TextState =
            {
                FontSize = 20,
                FontStyle = FontStyles.Bold,
                ForegroundColor = Color.FromRgb(System.Drawing.Color.Black)
            },
            HorizontalAlignment = HorizontalAlignment.Center
        };

        page.Paragraphs.Add(title);
        page.Paragraphs.Add(table);

        document.Save(savePath);
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