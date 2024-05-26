using Microsoft.AspNetCore.Components;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.Dialogs;

public partial class ImportSessionInfoDialog : ComponentBase
{
    [CascadingParameter] public required MudDialogInstance MudDialog { get; set; }

    [Parameter] public List<SessionImportModel> NewSessions { get; set; } = [];

    [Parameter] public List<SessionImportModel> UpdatedSessions { get; set; } = [];
}