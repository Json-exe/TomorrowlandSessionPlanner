﻿namespace TomorrowlandSessionPlanner.Models;

public class OverlappingSessionClass
{
    public Session Session { get; set; } = new();
    public List<Session> OverlappingSessions { get; set; } = new();
}