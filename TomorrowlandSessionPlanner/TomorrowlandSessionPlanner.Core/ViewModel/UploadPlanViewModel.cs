using System.Text.Json;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Code;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Core.ViewModel;

public partial class UploadPlanViewModel : Model.ViewModel
{
    private readonly PlannerManager _plannerManager;
    private readonly NavigationManager _navigationManager;
    private readonly ISnackbar _snackbar;

    public UploadPlanViewModel(PlannerManager plannerManager, NavigationManager navigationManager, ISnackbar snackbar)
    {
        _plannerManager = plannerManager;
        _navigationManager = navigationManager;
        _snackbar = snackbar;
    }

    [RelayCommand]
    private async Task UploadFile(IBrowserFile file)
    {
        try
        {
            if (!file.Name.EndsWith(".tmlplanner"))
            {
                throw new ArgumentException("Wrong File Format!", nameof(file));
            }
            var fileContent = file.OpenReadStream();
            var json = await new StreamReader(fileContent).ReadToEndAsync();
            var sessions = JsonSerializer.Deserialize<List<Session>>(json);
            if (sessions == null)
            {
                throw new InvalidOperationException("Sessions is null!");
            }
            _plannerManager.AddedSessions.Clear();
            _plannerManager.AddedSessions.AddRange(sessions);
            _navigationManager.NavigateTo("/SessionAnalyzer");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _snackbar.Add("Es ist ein Fehler beim laden der Datei aufgetreten! " +
                          "Bitte stelle sicher das dies die Richtige Datei ist!", Severity.Error);
        }
    }
}