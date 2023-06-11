using Aspose.Pdf;
using Aspose.Pdf.Text;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using TomorrowlandSessionPlanner.Code;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Result
{
    private bool _loading = true;
    private List<Session> _sortedSessions = new();
    private IJSObjectReference _jsModule = null!;
    private readonly DateTime _weekend2Start = new(2023, 7, 28, 00, 0, 0);
    private readonly DateTime _weekend1Start = new(2023, 7, 21, 00, 0, 0);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        _jsModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./Pages/Result.razor.js");
        if (firstRender)
        {
            _sortedSessions = PlannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
            await Task.Delay(2500);
            _loading = false;
            StateHasChanged();
        }
    }

    private async void DownloadFile()
    {
        var json = JsonConvert.SerializeObject(_sortedSessions, Formatting.Indented);
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.tmlplanner");
        await File.WriteAllTextAsync(savePath, json);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async void DownloadHtmlFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.html");
        var html = await new HTMLCreator().CreateHTMLTable(_sortedSessions, PlannerManager);
        await File.WriteAllTextAsync(savePath, html);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async void DownloadPdfFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.pdf");
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
            var djName = PlannerManager._djList.FirstOrDefault(d => d.id == sortedSession.DJId)?.Name;
            var stageName = PlannerManager._stageList.FirstOrDefault(s => s.id == sortedSession.StageId)?.Name;
            var row = table.Rows.Add();
            row.Cells.Add(stageName);
            row.Cells.Add(djName);
            row.Cells.Add(sortedSession.StartTime.ToString("dd-MM-yyyy HH:mm"));
            row.Cells.Add(sortedSession.EndTime.ToString("dd-MM-yyyy HH:mm"));
        }
        
        // Create a title text for the PDF
        var title = new TextFragment("Tomorrowland Session Planner");
        title.TextState.FontSize = 20;
        title.TextState.FontStyle = FontStyles.Bold;
        title.TextState.ForegroundColor = Color.FromRgb(System.Drawing.Color.Black);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        
        page.Paragraphs.Add(title);
        page.Paragraphs.Add(table);
        
        document.Save(savePath);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        await _jsModule.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }
}