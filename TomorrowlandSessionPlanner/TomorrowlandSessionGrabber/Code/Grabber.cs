using System.Collections.ObjectModel;
using System.Globalization;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using TomorrowlandSessionGrabber.Models;

namespace TomorrowlandSessionGrabber.Code;

public class Grabber
{
    private const string BaseUrl = "https://www.tomorrowland.com/en/festival/line-up/stages/";
    private ReadOnlyCollection<IWebElement>? _eventDayButtons;
    private IWebDriver _driver = null!;
    private int _selectedDay;
    private readonly List<Session> _sessions = [];

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
        // driver.ExecuteScript("Cookiebot.dialog.submitConsent()");
        if (grabberOptions.Weekend == Weekend.Weekend1)
        {
            var weekendSwitch = driver.FindElement(By.ClassName("weekend-switch"));
            var weekend1Button = weekendSwitch.FindElement(By.ClassName("weekend1"));
            weekend1Button.Click();
            var eventDaysWrapper = driver.FindElement(By.ClassName("eventdays-wrapper"));
            var eventDays = eventDaysWrapper.FindElement(By.XPath(".//div[@data-weekend='1']"));
            _eventDayButtons = eventDays.FindElements(By.XPath(".//div"));
            Console.WriteLine("Grabbing weekend 1...");
            GetStages();
        }
        else
        {
            var weekendSwitch = driver.FindElement(By.ClassName("weekend-switch"));
            var weekend2Button = weekendSwitch.FindElement(By.ClassName("weekend2"));
            weekend2Button.Click();
            var eventDaysWrapper = driver.FindElement(By.ClassName("eventdays-wrapper"));
            var eventDays = eventDaysWrapper.FindElement(By.XPath(".//div[@data-weekend='2']"));
            // Inside eventDays there are 3 divs, one for each day. They have a data-date attribute with the date. Get the 3 divs
            _eventDayButtons = eventDays.FindElements(By.XPath(".//div"));
            Console.WriteLine("Grabbing weekend 2...");
            GetStages();
        }

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
        var lineupDiv = _driver.FindElement(By.ClassName("lineup__page--stages"));
        var eventDaysLineUp = lineupDiv.FindElement(By.ClassName("eventdays"));
        var eventDay = eventDaysLineUp.FindElements(By.ClassName("eventday")).First(x => x.Displayed);
        var stages = eventDay.FindElements(By.ClassName("stage"));
        EnumerateStages(stages);
        DaySwitch();
    }

    private void DaySwitch()
    {
        _selectedDay++;
        if (_selectedDay >= _eventDayButtons?.Count) return;
        var button = _eventDayButtons![_selectedDay];
        button.Click();
        Thread.Sleep(500);
        GetStages();
    }

    private void EnumerateStages(ReadOnlyCollection<IWebElement> stages)
    {
        foreach (var stage in stages)
        {
            var heading = stage.FindElement(By.ClassName("stage__heading")).FindElement(By.TagName("h4"));
            var content = stage.FindElement(By.ClassName("stage__content")).FindElements(By.TagName("li"));
            foreach (var stageContent in content)
            {
                var session = new Session
                {
                    StageName = heading.Text
                };
                var djData = stageContent.FindElement(By.TagName("a"));

                ApplySessionTimeStampIfAvailable(session, djData);

                var djName = djData.Text;
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

                session.DjName = djName.Trim();
                Console.WriteLine(
                    $"Stage: {session.StageName}, DJ: {session.DjName}, Start: {session.StartTime}, End: {session.EndTime}");
                _sessions.Add(session);
            }
        }
    }

    private void ApplySessionTimeStampIfAvailable(Session session, IWebElement djData)
    {
        try
        {
            var timeSlot = djData.FindElement(By.TagName("span"));
            var timeSlotParts = timeSlot.Text.Split(" - ");
            // The date is not in the time slot, so we need to get it from the event day button. The event day button has a data-date attribute with the date
            var date = _eventDayButtons![_selectedDay].GetAttribute("data-date");
            // Format is: 2023/07/28
            var dateParts = date.Split("/");
            var endParts = dateParts.ToArray();
            var startTime = TimeSpan.Parse(timeSlotParts[0], CultureInfo.InvariantCulture);
            var endTime = TimeSpan.Parse(timeSlotParts[1], CultureInfo.InvariantCulture);

            // Check if end or start time is after midnight. If so, add a day to the date
            if (endTime < startTime)
            {
                // End time is after midnight, so add a day to the date
                var dateTime = new DateTime(int.Parse(endParts[0]), int.Parse(endParts[1]),
                    int.Parse(endParts[2]));
                dateTime = dateTime.AddDays(1);
                endParts[0] = dateTime.Year.ToString();
                endParts[1] = dateTime.Month.ToString();
                endParts[2] = dateTime.Day.ToString();
            }

            if (endTime >= new TimeSpan(0, 0, 0) && startTime >= new TimeSpan(0, 0, 0)
                                                 && endTime <= new TimeSpan(2, 0, 0) &&
                                                 startTime <= new TimeSpan(2, 0, 0))
            {
                // Start and end time are between 00:00 and 02:00, so add a day to both dates
                var dateTime = new DateTime(int.Parse(endParts[0]), int.Parse(endParts[1]),
                    int.Parse(endParts[2]));
                dateTime = dateTime.AddDays(1);
                endParts[0] = dateTime.Year.ToString();
                endParts[1] = dateTime.Month.ToString();
                endParts[2] = dateTime.Day.ToString();

                dateTime = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]),
                    int.Parse(dateParts[2]));
                dateTime = dateTime.AddDays(1);
                dateParts[0] = dateTime.Year.ToString();
                dateParts[1] = dateTime.Month.ToString();
                dateParts[2] = dateTime.Day.ToString();
            }

            session.StartTime = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]),
                int.Parse(dateParts[2]), startTime.Hours, startTime.Minutes, startTime.Seconds);
            session.EndTime = new DateTime(int.Parse(endParts[0]), int.Parse(endParts[1]),
                int.Parse(endParts[2]), endTime.Hours, endTime.Minutes, endTime.Seconds);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error applying time stamp: " + e.Message +
                              ". Adding session without time stamp. Applying only date for now...");
            var date = _eventDayButtons![_selectedDay].GetAttribute("data-date");
            var dateParts = date.Split("/");
            session.StartTime = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]),
                int.Parse(dateParts[2]));
            session.EndTime = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[1]),
                int.Parse(dateParts[2]));
        }
    }
}