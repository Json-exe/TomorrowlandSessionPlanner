using TomorrowlandSessionGrabber.Code;
using TomorrowlandSessionGrabber.Models;

namespace TomorrowlandSessionGrabber
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            new Grabber().Start(new GrabberOptions() { Headless = true, Weekend = Weekend.Weekend1 });
        }
    }
}