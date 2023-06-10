using Microsoft.Data.Sqlite;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Code;

public class PlannerManager
{
    public readonly List<Session> AddedSessions = new();
    public readonly List<Dj> _djList = new();
    public readonly List<Stage> _stageList = new();
    public readonly List<Session> _sessionList = new();

    public async Task Init()
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
    }
}