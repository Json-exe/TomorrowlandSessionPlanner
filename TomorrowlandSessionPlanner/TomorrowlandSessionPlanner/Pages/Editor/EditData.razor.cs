using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.Sqlite;
using MudBlazor;
using Newtonsoft.Json;
using TomorrowlandSessionPlanner.Dialogs;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages.Editor;

public partial class EditData : ComponentBase
{
    private readonly List<Dj> _djList = new();
    private readonly List<Stage> _stageList = new();
    private readonly List<Session> _sessionList = new();
    private bool _loading = true;
    private int _selectedStage;
    private DateTime? _startDate;
    private string? _newDj;
    private const bool IsAccessible = true;

    private async void AddSession()
    {
        var parameters = new DialogParameters
        {
            { "Djs", _djList },
            { "Stages", _stageList },
            { "PreSelectedStage", _selectedStage },
            { "PreSelectedDate", _startDate }
        };
        var dialog = await DialogService.ShowAsync<AddSessionDialog>("Add Session", parameters,
            new DialogOptions
                { CloseButton = false, FullWidth = true, CloseOnEscapeKey = false, DisableBackdropClick = true });
        var result = await dialog.Result;
        if (result.Canceled) return;
        var session = (Session)result.Data;
        await AddSessionToDatabase(session);
        StateHasChanged();
    }

    private async Task AddSessionToDatabase(Session session)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        var command =
            new SqliteCommand(
                "INSERT INTO Sessions (StageId, DJId, StartTime, EndTime) VALUES (@StageId, @DJId, @StartTime, @EndTime)",
                connection);
        command.Parameters.AddWithValue("@StageId", session.StageId);
        command.Parameters.AddWithValue("@DJId", session.DjId);
        command.Parameters.AddWithValue("@StartTime", session.StartTime);
        command.Parameters.AddWithValue("@EndTime", session.EndTime);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        _sessionList.Add(session);
        Snackbar.Add("Session added to database", Severity.Success);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!IsAccessible)
        {
            NavigationManager.NavigateTo("/");
            return;
        }

        if (firstRender)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
            await using var connection = new SqliteConnection($"Data Source={databasePath}");
            await connection.OpenAsync();

            var command = new SqliteCommand("SELECT * FROM DJs", connection);
            var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _djList.Add(new Dj
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            await reader.CloseAsync();

            command = new SqliteCommand("SELECT * FROM Stages", connection);
            reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _stageList.Add(new Stage
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                });
            }

            await reader.CloseAsync();

            command = new SqliteCommand("SELECT * FROM Sessions", connection);

            reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                _sessionList.Add(new Session
                {
                    Id = reader.GetInt32(0),
                    StageId = reader.GetInt32(1),
                    DjId = reader.GetInt32(2),
                    StartTime = reader.GetDateTime(3),
                    EndTime = reader.GetDateTime(4)
                });
            }

            await reader.CloseAsync();
            await connection.CloseAsync();
            _loading = false;
            StateHasChanged();
        }
    }

    private async void ImportDj(IBrowserFile? file)
    {
        try
        {
            if (file == null)
            {
                throw new Exception("No File selected!");
            }

            if (!file.Name.EndsWith(".json"))
            {
                throw new Exception("Wrong File Format!");
            }

            var fileContent = file.OpenReadStream();
            var json = await new StreamReader(fileContent).ReadToEndAsync();
            var sessionsToImport = JsonConvert.DeserializeObject<List<SessionImportModel>>(json);
            if (sessionsToImport == null)
            {
                throw new Exception("No Sessions found!");
            }

            Snackbar.Add(
                $"Importing {sessionsToImport.Count} DJs this could take a while... Please wait until the import is finished!",
                Severity.Info);
            await AnalyzeImportDJs(sessionsToImport);
            Snackbar.Add("Import finished!", Severity.Success);
            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Snackbar.Add(
                "Es ist ein Fehler beim importieren der Datei aufgetreten! Bitte stelle sicher das dies die Richtige Datei ist!",
                Severity.Error);
        }
    }

    private async Task AnalyzeImportDJs(List<SessionImportModel> dj)
    {
        var newDjs = new List<SessionImportModel>();
        var newDjsData = new List<Dj>();
        foreach (var djImportModel in dj)
        {
            if (_djList.Any(x => x.Name == djImportModel.DjName) || newDjs.Any(x => x.DjName == djImportModel.DjName)) continue;
            var newDj = new Dj
            {
                Name = djImportModel.DjName
            };
            newDjsData.Add(newDj);
            newDjs.Add(djImportModel);
        }
        
        Snackbar.Add($"New Djs imports: {newDjs.Count}", Severity.Success);
        var dialog = await DialogService.ShowAsync<ImportSessionInfoDialog>("Import Info", new DialogParameters
        {
            { "newSessions", newDjs },
        }, new DialogOptions
        {
            CloseButton = false,
            FullWidth = true,
            CloseOnEscapeKey = false,
            DisableBackdropClick = true
        });
        var result = await dialog.Result;
        if (result.Canceled) return;
        foreach (var djData in newDjsData)
        {
            await AddDj(djData);
        }
    }

    private async void ImportSessions(IBrowserFile? file)
    {
        try
        {
            if (file == null)
            {
                throw new Exception("No File selected!");
            }

            if (!file.Name.EndsWith(".json"))
            {
                throw new Exception("Wrong File Format!");
            }

            var fileContent = file.OpenReadStream();
            var json = await new StreamReader(fileContent).ReadToEndAsync();
            var sessionsToImport = JsonConvert.DeserializeObject<List<SessionImportModel>>(json);
            if (sessionsToImport == null)
            {
                throw new Exception("No Sessions found!");
            }

            Snackbar.Add(
                $"Importing {sessionsToImport.Count} Sessions this could take a while... Please wait until the import is finished!",
                Severity.Info);
            await AnalyzeImportSessions(sessionsToImport);
            Snackbar.Add("Import finished!", Severity.Success);
            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Snackbar.Add(
                "Es ist ein Fehler beim importieren der Datei aufgetreten! Bitte stelle sicher das dies die Richtige Datei ist!",
                Severity.Error);
        }
    }

    private async Task AnalyzeImportSessions(List<SessionImportModel> sessions)
    {
        var failedSessions = new List<SessionImportModel>();
        var updatedSessions = new List<SessionImportModel>();
        var newSessions = new List<SessionImportModel>();
        var newSessionsImportData = new List<Session>();
        var updatedSessionsImportData = new List<Session>();
        foreach (var sessionImportModel in sessions)
        {
            try
            {
                sessionImportModel.StageName = sessionImportModel.StageName.ToUpper() == "CORE" ? sessionImportModel.StageName.ToUpper() : sessionImportModel.StageName;
                var stage = _stageList.First(x => x.Name == sessionImportModel.StageName);
                var dj = _djList.First(x => x.Name == sessionImportModel.DjName);
                var session = new Session
                {
                    StageId = stage.Id,
                    DjId = dj.Id,
                    StartTime = sessionImportModel.StartTime,
                    EndTime = sessionImportModel.EndTime
                };
                if (_sessionList.Any(x =>
                        x.StageId == session.StageId && x.StartTime == session.StartTime &&
                        x.EndTime == session.EndTime))
                {
                    // Check if the dj is the same
                    var existingSession = _sessionList.First(x =>
                        x.StageId == session.StageId && x.StartTime == session.StartTime &&
                        x.EndTime == session.EndTime);

                    if (existingSession.DjId != session.DjId)
                    {
                        updatedSessions.Add(sessionImportModel);
                        updatedSessionsImportData.Add(session);
                        continue;
                    }

                    failedSessions.Add(sessionImportModel);
                    continue;
                }

                newSessions.Add(sessionImportModel);
                newSessionsImportData.Add(session);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                failedSessions.Add(sessionImportModel);
            }
        }

        Snackbar.Add($"Failed Session imports: {failedSessions.Count} Likely they exist already", Severity.Error);
        Snackbar.Add($"Updated Session imports: {updatedSessions.Count} Data that is in the database but is incorrect",
            Severity.Warning);
        Snackbar.Add($"New Session imports: {newSessions.Count}", Severity.Success);
        var dialog = await DialogService.ShowAsync<ImportSessionInfoDialog>("Import Info", new DialogParameters
        {
            { "newSessions", newSessions },
            { "updatedSessions", updatedSessions }
        }, new DialogOptions
        {
            CloseButton = false,
            FullWidth = true,
            CloseOnEscapeKey = false,
            DisableBackdropClick = true
        });
        var result = await dialog.Result;
        if (result.Canceled) return;
        foreach (var session in newSessionsImportData)
        {
            await AddSessionToDatabase(session);
        }

        foreach (var sessionToUpdate in updatedSessionsImportData)
        {
            await UpdateSessionInDatabase(sessionToUpdate);
        }
    }

    private async Task UpdateSessionInDatabase(Session session)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        var command =
            new SqliteCommand(
                "UPDATE Sessions SET DJId = @DJId WHERE StageId = @StageId AND StartTime = @StartTime AND EndTime = @EndTime",
                connection);
        command.Parameters.AddWithValue("@StageId", session.StageId);
        command.Parameters.AddWithValue("@DJId", session.DjId);
        command.Parameters.AddWithValue("@StartTime", session.StartTime);
        command.Parameters.AddWithValue("@EndTime", session.EndTime);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        _sessionList.First(x =>
                x.StageId == session.StageId && x.StartTime == session.StartTime &&
                x.EndTime == session.EndTime)
            .DjId = session.DjId;
        Snackbar.Add("Session updated in database", Severity.Success);
    }

    private async Task AddDj(Dj dj)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        var command =
            new SqliteCommand(
                "INSERT INTO DJs (Name) VALUES (@DJName)",
                connection);
        command.Parameters.AddWithValue("@DJName", dj.Name);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        _djList.Add(dj);
        Snackbar.Add("DJ added to database", Severity.Success);
    }

    private async void AddNewDj()
    {
        if (string.IsNullOrEmpty(_newDj) || _djList.Any(x => x.Name == _newDj)) return;
        var dj = new Dj
        {
            Name = _newDj
        };
        await AddDj(dj);
    }
}