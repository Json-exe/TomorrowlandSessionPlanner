using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Newtonsoft.Json;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages;

public partial class UploadPlan
{
    private async void UploadFiles(IBrowserFile file)
    {
        try
        {
            if (file == null)
            {
                throw new Exception("No File selected!");
            }
            if (!file.Name.EndsWith(".tmlplanner"))
            {
                throw new Exception("Wrong File Format!");
            }
            var fileContent = file.OpenReadStream();
            var json = await new StreamReader(fileContent).ReadToEndAsync();
            var sessions = JsonConvert.DeserializeObject<List<Session>>(json);
            if (sessions == null)
            {
                throw new Exception("No Sessions found!");
            }
            PlannerManager.AddedSessions.Clear();
            PlannerManager.AddedSessions.AddRange(sessions);
            NavigationManager.NavigateTo("/SessionAnalyzer");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Snackbar.Add("Es ist ein Fehler beim laden der Datei aufgetreten! Bitte stelle sicher das dies die Richtige Datei ist!", Severity.Error);
        }
    }
}