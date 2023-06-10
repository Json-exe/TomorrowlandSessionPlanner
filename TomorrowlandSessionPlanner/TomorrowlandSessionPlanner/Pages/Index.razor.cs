using Microsoft.Data.Sqlite;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Pages;

public partial class Index
{
    private readonly List<Dj> _djList = new();
    private readonly List<Stage> _stageList = new();
    private readonly List<Session> _sessionList = new();
    private List<Session> _filteredSessions = new();
    private readonly DateTime _weekend2Start = new(2023, 7, 28, 00, 0, 0);
    private readonly DateTime _weekend1Start = new(2023, 7, 21, 00, 0, 0);
    private IEnumerable<Stage> _stageFilter = new List<Stage>();
    private string? _djFilter;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
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
            _filteredSessions.AddRange(_sessionList);
            StateHasChanged();
        }
    }
    
    private Task<IEnumerable<string>> DjSearch(string arg)
    {
        return Task.FromResult(string.IsNullOrEmpty(arg) ? _djList.Select(x => x.Name) : _djList.Where(x => x.Name.Contains(arg, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Name));
    }
    
    private void ApplyFilters()
    {
        if (!_stageFilter.Any() && string.IsNullOrEmpty(_djFilter))
        {
            _filteredSessions = _sessionList;
            StateHasChanged();
            return;
        }
        var dj = _djList.FirstOrDefault(x => _djFilter != null && x.Name.Contains(_djFilter, StringComparison.InvariantCultureIgnoreCase));
        // Filter by stage and dj
        if (_stageFilter.Any() && !string.IsNullOrEmpty(_djFilter))
        {
            if (dj == null) return;
            _filteredSessions = _sessionList.Where(x => _stageFilter.Any(y => y.id == x.StageId) && x.DJId == dj.id).ToList();
            StateHasChanged();
            return;
        }
        // Filter by stage
        if (_stageFilter.Any())
        {
            _filteredSessions = _sessionList.Where(x => _stageFilter.Any(y => y.id == x.StageId)).ToList();
            StateHasChanged();
            return;
        }
        // Filter by dj
        if (dj == null) return;
        _filteredSessions = _sessionList.Where(x => _djFilter != null && x.DJId == dj.id).ToList();
        StateHasChanged();
    }
    
    private void AddUserSession(Session session)
    {
        PlannerManager.AddedSessions.Add(session);
    }
    
    private void RemoveUserSession(Session session)
    {
        PlannerManager.AddedSessions.Remove(session);
    }
}