using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public enum Weekdays { Planting, Grow1, Grow2, Harvest, Market};
    public static string[] WeekdaysText = new string[] {"Plantsday", "Tendsday", "Harvest Eve", "Harvest", "Marketfall" };
    public enum Seasons { Spring, Summer, Fall, Winter};
    public static bool Paused = false;
    public static int DayCounter = 0;
    public static Weekdays DayOfTheWeek = Weekdays.Planting;
    public static Seasons Season = Seasons.Summer;
    public int SkipDays = 0;
    
    public static void Pause()
    {
        Paused = true;
        Time.timeScale = 0;
    }
    public static void Unpause()
    {
        Paused = false;
        Time.timeScale = 1;
    }
    public static void AdvanceDay()
    {
        DayCounter++;
        UpdateDay(true);
        if (FarmPlot.current)
            FarmPlot.current.AdvanceDay();
        Debug.Log($"Sleeping! Now the day is {WeekdaysText[(int)DayOfTheWeek]}");

    }

    public static void UpdateDay(bool display = false)
    {
        DayOfTheWeek = (Weekdays)(DayCounter % System.Enum.GetNames(typeof(Weekdays)).Length);
        if (display)
        {
            HUDManager.current.SeasonTitleText.gameObject.SetActive(true);
            HUDManager.current.WeekdayTitleText.text = WeekdaysText[(int)DayOfTheWeek];
            HUDManager.current.WeekdayTitleText.gameObject.SetActive(true);
        }

    }

    public void Sleep()
    {
        AdvanceDay();
        for (int i = 0; i < SkipDays; i++)
        {
            AdvanceDay();
        }
    }
}
