using Microsoft.Data.Sqlite;
using TomorrowlandSessionPlanner.Models;

namespace TomorrowlandSessionPlanner.Code;

public class PlannerManager
{
    public readonly List<Session> AddedSessions = new();
    public readonly List<Dj> DjList = new();
    public readonly List<Stage> StageList = new();
    public readonly List<Session> SessionList = new();

    public async Task Init()
    {
        if (DjList.Any() && StageList.Any() && SessionList.Any()) return;
        
        var currentDirectory = Directory.GetCurrentDirectory();
        var databasePath = Path.Combine(currentDirectory, "Data", "tmldata.db");
        await using var connection = new SqliteConnection($"Data Source={databasePath}");
        await connection.OpenAsync();

        var command = new SqliteCommand("SELECT * FROM DJs", connection);
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            DjList.Add(new Dj
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
            StageList.Add(new Stage
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
            SessionList.Add(new Session
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