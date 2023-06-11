using Microsoft.Data.Sqlite;
using MudBlazor;
using TomorrowlandSessionPlanner.Dialogs;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages.Editor;

public partial class EditData
{
    private readonly List<Dj> _djList = new();
    private readonly List<Stage> _stageList = new();
    private readonly List<Session> _sessionList = new();
    private bool _loading = true;
    private int _selectedStage;
    private DateTime? _startDate;
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
        var dialog = await DialogService.ShowAsync<AddSessionDialog>("Add Session", parameters, new DialogOptions { CloseButton = false, FullWidth = true, CloseOnEscapeKey = false, DisableBackdropClick = true });
        var result = await dialog.Result;
        if (result.Canceled) return;
        var session = (Session) result.Data;
        _sessionList.Add(session);
        await AddSessionToDatabase(session);
        StateHasChanged();
    }

    private async Task AddSessionToDatabase(Session session)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();
        var command = new SqliteCommand("INSERT INTO Sessions (StageId, DJId, StartTime, EndTime) VALUES (@StageId, @DJId, @StartTime, @EndTime)", connection);
        command.Parameters.AddWithValue("@StageId", session.StageId);
        command.Parameters.AddWithValue("@DJId", session.DJId);
        command.Parameters.AddWithValue("@StartTime", session.StartTime);
        command.Parameters.AddWithValue("@EndTime", session.EndTime);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
        @Snackbar.Add("Session added to database", Severity.Success);
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
                    id = reader.GetInt32(0),
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
                    id = reader.GetInt32(0),
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
                    id = reader.GetInt32(0),
                    StageId = reader.GetInt32(1),
                    DJId = reader.GetInt32(2),
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
}