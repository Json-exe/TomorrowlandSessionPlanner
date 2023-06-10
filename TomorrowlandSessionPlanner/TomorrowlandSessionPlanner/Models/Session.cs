namespace TomorrowlandSessionPlanner.Models;

public class Session
{
    public int id { get; set; }
    public int StageId { get; set; }
    public int DJId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}