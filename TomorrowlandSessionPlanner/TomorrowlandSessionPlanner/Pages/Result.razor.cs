using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using TomorrowlandSessionPlanner.Components;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Model;
using TomorrowlandSessionPlanner.Core.ViewModel;
using TomorrowlandSessionPlanner.Dialogs;
using Colors = QuestPDF.Helpers.Colors;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Result : ViewModelComponent<ResultViewModel>
{
    private readonly DateTime _weekend2Start = new(2023, 7, 28);
    private readonly DateTime _weekend1Start = new(2023, 7, 21);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ViewModel.InitResultPageCommand.ExecuteAsync(null);
        }
    }

    private async Task ShowSupplements()
    {
        var allSessions = PlannerManager.SessionList.Where(s => !PlannerManager.AddedSessions.Any(ss =>
            IsSessionOverlapping(s, ss) && PlannerManager.AddedSessions.Exists(session => session.Id != s.Id))).ToList();
        var dialogParameters = new DialogParameters
        {
            { "supplementSessions", allSessions }
        };
        var dialogReference = await DialogService.ShowAsync<SupplementSessionsDialog>("Supplement Sessions",
            dialogParameters,
            new DialogOptions
            {
                CloseButton = false, MaxWidth = MaxWidth.ExtraExtraLarge, FullWidth = true, CloseOnEscapeKey = false,
                DisableBackdropClick = true
            });
        await dialogReference.Result;
        ViewModel.SortedSessions = PlannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
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
        var json = JsonSerializer.Serialize(ViewModel.SortedSessions);
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.tmlplanner");
        await File.WriteAllTextAsync(savePath, json);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (ViewModel.ObjectReference != null)
            await ViewModel.ObjectReference.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async Task DownloadHtmlFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.html");
        var html = await new HtmlCreator().CreateHtmlTable(ViewModel.SortedSessions, PlannerManager);
        await File.WriteAllTextAsync(savePath, html);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (ViewModel.ObjectReference != null)
            await ViewModel.ObjectReference.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    private async Task GeneratePdfFile()
    {
        var document = Document.Create(documentContainer =>
        {
            documentContainer.Page(descriptor =>
            {
                descriptor.Size(PageSizes.A4);
                descriptor.DefaultTextStyle(style =>
                {
                    style.FontSize(12);
                    return style;
                });
                descriptor.Margin(2, Unit.Centimetre);
                const string fontColor = "#4b0076";
                descriptor.Header()
                    .Text("Dein Plan")
                    .Bold()
                    .Underline()
                    .FontSize(38)
                    .FontColor(fontColor);

                descriptor.Content().Table(tableDescriptor =>
                {
                    tableDescriptor.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    tableDescriptor.Header(cellDescriptor =>
                    {
                        cellDescriptor.Cell().Element(CellStyle).Text("Stage").Bold().FontSize(20).FontColor(fontColor);
                        cellDescriptor.Cell().Element(CellStyle).Text("Artist").Bold().FontSize(20)
                            .FontColor(fontColor);
                        cellDescriptor.Cell().Element(CellStyle).Text("Start").Bold().FontSize(20).FontColor(fontColor);
                        cellDescriptor.Cell().Element(CellStyle).Text("Ende").Bold().FontSize(20).FontColor(fontColor);
                        IContainer CellStyle(IContainer container) => DefaultCellStyle(container, Colors.Grey.Lighten3);
                    });

                    for (uint i = 1; i <= ViewModel.SortedSessions.Count; i++)
                    {
                        var session = ViewModel.SortedSessions[(int)i - 1];
                        tableDescriptor.Cell().Row(i)
                            .Column(1).Element(CellStyle).Text(session.Stage!.Name);
                        tableDescriptor.Cell().Row(i)
                            .Column(2).Element(CellStyle).Text(session.Dj!.Name);
                        tableDescriptor.Cell().Row(i)
                            .Column(3).Element(CellStyle).Text(session.StartTime.ToString("dd.M - HH:mm"));
                        tableDescriptor.Cell().Row(i)
                            .Column(4).Element(CellStyle).Text(session.EndTime.ToString("dd.M - HH:mm"));

                        IContainer CellStyle(IContainer container) =>
                            DefaultCellStyle(container, Colors.White).ShowOnce();
                    }

                    IContainer DefaultCellStyle(IContainer container, string backgroundColor)
                    {
                        return container
                            .Border(1)
                            .BorderColor(Colors.Grey.Lighten1)
                            .Background(backgroundColor)
                            .PaddingVertical(5)
                            .PaddingHorizontal(10)
                            .AlignCenter()
                            .AlignMiddle();
                    }
                });
            });
        });

        QuestPDF.Settings.License = LicenseType.Community;
        var pdfData = document.GeneratePdf();
        if (ViewModel.ObjectReference != null)
            await ViewModel.ObjectReference.InvokeVoidAsync("saveAsFile", "TML-Session-Planner-Plan.pdf", Convert.ToBase64String(pdfData));
    }

    public async ValueTask DisposeAsync()
    {
        if (ViewModel.ObjectReference != null) await ViewModel.ObjectReference.DisposeAsync();
    }
}