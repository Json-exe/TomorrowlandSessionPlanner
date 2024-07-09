using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Dialogs;
using TomorrowlandSessionPlanner.Core.Model;
using Colors = QuestPDF.Helpers.Colors;
using Document = QuestPDF.Fluent.Document;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public partial class ResultViewModel : Model.ViewModel
{
    [ObservableProperty] private IJSObjectReference? _objectReference;
    [ObservableProperty] private List<Session> _sortedSessions = [];

    private readonly IJSRuntime _jsRuntime;
    private readonly PlannerManager _plannerManager;
    private readonly NavigationManager _navigationManager;
    private readonly IDialogService _dialogService;

    public ResultViewModel(IJSRuntime jsRuntime, PlannerManager plannerManager, NavigationManager navigationManager,
        IDialogService dialogService)
    {
        _jsRuntime = jsRuntime;
        _plannerManager = plannerManager;
        _navigationManager = navigationManager;
        _dialogService = dialogService;
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

    [RelayCommand]
    private async Task ShowSupplements(CancellationToken cancellationToken)
    {
        var allSessions = _plannerManager.SessionList
            .Where(s => !_plannerManager.AddedSessions.Exists(ads => ads.Id == s.Id))
            .Where(s => !_plannerManager.AddedSessions.Exists(ads => IsSessionOverlapping(s, ads)))
            .ToList();
        var dialogParameters = new DialogParameters
        {
            { "supplementSessions", allSessions }
        };
        var dialogReference = await _dialogService.ShowAsync<SupplementSessionsDialog>("Supplement Sessions",
            dialogParameters,
            new DialogOptions
            {
                CloseButton = false, MaxWidth = MaxWidth.ExtraExtraLarge, FullWidth = true, CloseOnEscapeKey = false,
                DisableBackdropClick = true
            });
        await dialogReference.Result;
        SortedSessions = _plannerManager.AddedSessions.OrderBy(s => s.StartTime).ToList();
    }

    private static bool IsSessionOverlapping(Session session1, Session session2)
    {
        // Überprüfen, ob das Ende von session1 nach dem Start von session2 liegt und
        // das Start von session1 vor dem Ende von session2 liegt
        return session1.EndTime > session2.StartTime && session1.StartTime < session2.EndTime;
    }

    #region PlanDownloads

    [RelayCommand]
    private async Task DownloadFile()
    {
        var json = JsonSerializer.Serialize(SortedSessions);
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.tmlplanner");
        await File.WriteAllTextAsync(savePath, json);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (ObjectReference != null)
            await ObjectReference.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    [RelayCommand]
    private async Task DownloadHtmlFile()
    {
        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "Data",
            $"SessionPlan{DateTime.Now:ddMMyyyyHHmmss}.html");
        var html = await new HtmlCreator().CreateHtmlTable(SortedSessions, _plannerManager);
        await File.WriteAllTextAsync(savePath, html);
        var bites = await File.ReadAllBytesAsync(savePath);
        var fileName = Path.GetFileName(savePath);
        if (ObjectReference != null)
            await ObjectReference.InvokeVoidAsync("saveAsFile", fileName, Convert.ToBase64String(bites));
        File.Delete(savePath);
    }

    [RelayCommand]
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

                    for (uint i = 1; i <= SortedSessions.Count; i++)
                    {
                        var session = SortedSessions[(int)i - 1];
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
        if (ObjectReference != null)
            await ObjectReference.InvokeVoidAsync("saveAsFile", "TML-Session-Planner-Plan.pdf",
                Convert.ToBase64String(pdfData));
    }

    #endregion

    public override ValueTask DisposeAsync()
    {
        return ObjectReference?.DisposeAsync() ?? ValueTask.CompletedTask;
    }
}