using Microsoft.AspNetCore.Components.Forms;
using TomorrowlandSessionPlanner.Components;
using TomorrowlandSessionPlanner.Core.ViewModel;

namespace TomorrowlandSessionPlanner.Pages;

public partial class UploadPlan : ViewModelComponent<UploadPlanViewModel>
{
    private async Task UploadFileChanged(IBrowserFile? file)
    {
        if (file is not null)
        {
            await ViewModel.UploadFileCommand.ExecuteAsync(file);
        }
    }
}