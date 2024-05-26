using TomorrowlandSessionPlanner.Components;
using TomorrowlandSessionPlanner.Core.ViewModel;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Result : ViewModelComponent<ResultViewModel>
{
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ViewModel.InitResultPageCommand.ExecuteAsync();
            StateHasChanged();
        }
    }

    private async Task ShowSupplements()
    {
        await ViewModel.ShowSupplementsCommand.ExecuteAsync();
    }

    private async Task DownloadPlan(DownloadOptions option)
    {
        switch (option)
        {
            case DownloadOptions.Html:
                await ViewModel.DownloadHtmlFileCommand.ExecuteAsync();
                break;
            case DownloadOptions.Pdf:
                await ViewModel.GeneratePdfFileCommand.ExecuteAsync();
                break;
            case DownloadOptions.Json:
                await ViewModel.DownloadFileCommand.ExecuteAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(option), option, null);
        }
    }
}

internal enum DownloadOptions
{
    Html,
    Pdf,
    Json
}