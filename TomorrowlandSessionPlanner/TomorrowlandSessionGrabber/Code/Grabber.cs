using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TomorrowlandSessionGrabber.Models;

namespace TomorrowlandSessionGrabber.Code;

public class Grabber
{
    private const string BaseUrl = "https://belgium.tomorrowland.com/en/line-up/?page=artists";
    private ReadOnlyCollection<IWebElement>? _eventDayButtons;
    private ChromeDriver _driver = null!;
    private int _selectedDay;
    private readonly List<Session> _sessions = [];
    private IWebElement? _liveLineupElement;

    public void Start(GrabberOptions grabberOptions)
    {
        Console.WriteLine("Starting grabber...");
        var options = new ChromeOptions();
        if (grabberOptions.Headless)
        {
            options.AddArguments("--headless");
        }

        var driver = new ChromeDriver(options);
        _driver = driver;
        driver.Navigate().GoToUrl(BaseUrl);
        Console.WriteLine("Press ANY to continue!");
        Console.ReadLine();
        _liveLineupElement = driver.FindElement(By.TagName("tml-live-lineup"));
        GetStages();
        SaveDataAsJson();
    }

    private void SaveDataAsJson()
    {
        Console.WriteLine("Saving data as JSON...");
        if (File.Exists("sessionGrabberSessions.json"))
        {
            File.Delete("sessionGrabberSessions.json");
        }

        var json = JsonConvert.SerializeObject(_sessions, Formatting.Indented);
        File.WriteAllText("sessionGrabberSessions.json", json);
    }

    private void GetStages()
    {
        var lineupDiv = _liveLineupElement!.FindElement(By.XPath(".//div[contains(@class, '_responsiveMasonry_')]"));
        var artistList = lineupDiv.FindElements(By.XPath(".//div[contains(@class, '_masonryItem_')]"));
        EnumerateArtists(artistList);
    }

    private void EnumerateArtists(ReadOnlyCollection<IWebElement> artistLists)
    {
        foreach (var artistList in artistLists)
        {
            var artists =
                artistList.FindElements(
                    By.XPath(".//div[contains(@class, '_list_')]/ul[contains(@class, '_artists_')]"));
            foreach (var artist in artists)
            {
                var djInformationButton = artist.FindElement(By.TagName("button"));
                
                ((IJavaScriptExecutor) _driver).ExecuteScript("arguments[0].scrollIntoView(true);", djInformationButton);
                
                Thread.Sleep(700);
                djInformationButton.Click();
                // Search for a div with _overlay_ as class that has a button with the class _close_button_ inside. I want to get the div not the button!
                var overlay = _liveLineupElement!.FindElement(
                    By.XPath(".//div[contains(@class, '_overlay_')]/button[contains(@class, '_close_button_')]/.."));
                var content = overlay.FindElement(By.XPath(".//div[contains(@class, '_content_')]"));
                var djName = content.FindElement(By.XPath(".//h1")).Text;
                var performanceContainer = content.FindElement(By.XPath(".//div[contains(@class, '_performances_')]"));
                var performances =
                    performanceContainer.FindElements(By.XPath(".//div[contains(@class, '_performance_')]"));

                // DJ Name CAN look like this: Daybreak Session: Claptone|\r\n12:00 - 13:00
                // We want only the Name: Claptone
                var djNameParts = djName.Split("\r");
                // Also the part in front of the name must be removed
                if (!djNameParts[0].Contains("KAS:ST") && !djNameParts[0].Contains("Myu:sa"))
                {
                    var djNameParts2 = djNameParts[0].Split(":");
                    djName = djNameParts2.Length > 1 ? djNameParts2[1] : djNameParts2[0];
                }
                else
                {
                    djName = djNameParts[0];
                }

                EnumeratePerformances(performances, djName.Trim());
                var closeButton = overlay.FindElement(By.TagName("button"));
                closeButton.Click();
            }
        }
    }

    private void EnumeratePerformances(ReadOnlyCollection<IWebElement> performances, string name)
    {
        foreach (var performance in performances)
        {
            // Get the text of the h2 inside the performance. But there is also a span inside the h2, and we dont want the text of the span
            var stageName = performance.FindElement(By.XPath(".//h2")).Text;
            var stageNameParts = stageName.Split("\r");
            stageName = stageNameParts[0].Trim();
            var timeElements = performance.FindElements(By.XPath(".//div[contains(@class, '_time_')]/p"));
            // The first element contains the day in following format: Sun Jul 28 2024 as an example
            // The second element contains the time slot in following format: 12:00 - 13:00
            var timeSlot = timeElements[1].Text;
            var timeSlotParts = timeSlot.Split('-');
            var startTime = TimeSpan.Parse(timeSlotParts[0], CultureInfo.InvariantCulture);
            var endTime = TimeSpan.Parse(timeSlotParts[1], CultureInfo.InvariantCulture);
            var dateParts = timeElements[0].Text.Split(" ");
            var startDate = new DateTime(int.Parse(dateParts[3]), GetMonthNumber(dateParts[1]),
                int.Parse(dateParts[2]));
            var session = new Session
            {
                StageName = NormalizeStageName(stageName),
                DjName = name
            };
            ApplySessionTimeStampIfAvailable(session, startTime, endTime, startDate);

            Console.WriteLine(
                $"Stage: {session.StageName}, DJ: {session.DjName}, Start: {session.StartTime}, End: {session.EndTime}");
            _sessions.Add(session);
        }
    }

    private string NormalizeStageName(string stageName)
    {
        return stageName.Replace("by Bud", "").Replace("by Coke Studio", "").Trim();
    }

    private static int GetMonthNumber(string datePart)
    {
        return datePart switch
        {
            "Jan" => 1,
            "Feb" => 2,
            "Mar" => 3,
            "Apr" => 4,
            "May" => 5,
            "Jun" => 6,
            "Jul" => 7,
            "Aug" => 8,
            "Sep" => 9,
            "Oct" => 10,
            "Nov" => 11,
            "Dec" => 12,
            _ => 0
        };
    }

    private void ApplySessionTimeStampIfAvailable(Session session, TimeSpan startTime, TimeSpan endTime,
        DateTime startDate)
    {
        // Format is: 2023/07/28
        var endDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, endTime.Hours, endTime.Minutes,
            endTime.Seconds);

        // Check if end or start time is after midnight. If so, add a day to the date
        if (endTime < startTime)
        {
            // End time is after midnight, so add a day to the date
            endDate = endDate.AddDays(1);
        }

        startDate = new DateTime(startDate.Year, startDate.Month, startDate.Day, startTime.Hours, startTime.Minutes,
            startTime.Seconds);
        session.StartTime = startDate;
        session.EndTime = endDate;
    }
}