using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using TomorrowlandSessionPlanner.Core.Model;

namespace TomorrowlandSessionPlanner.Pages;

public partial class UploadPlan : ComponentBase
{
    private async Task UploadFiles(IBrowserFile? file)
    {
        try
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "No File selected!");
            }
            if (!file.Name.EndsWith(".tmlplanner"))
            {
                throw new ArgumentException("Wrong File Format!", nameof(file));
            }
            var fileContent = file.OpenReadStream();
            var json = await new StreamReader(fileContent).ReadToEndAsync();
            var sessions = JsonSerializer.Deserialize<List<Session>>(json);
            if (sessions == null)
            {
                throw new NullReferenceException("No Sessions found!");
            }
            PlannerManager.AddedSessions.Clear();
            PlannerManager.AddedSessions.AddRange(sessions);
            NavigationManager.NavigateTo("/SessionAnalyzer");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Snackbar.Add("Es ist ein Fehler beim laden der Datei aufgetreten! " +
                         "Bitte stelle sicher das dies die Richtige Datei ist!", Severity.Error);
        }
    }
}