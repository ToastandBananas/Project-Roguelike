using UnityEngine;

public class TimeSystem : MonoBehaviour
{
    public static int currentHour = 9;
    public static int currentMinute;
    public static int currentSecond;

    public const int defaultTimeTickInSeconds = 6;

    public static void IncreaseTime()
    {
        IncreaseCurrentSecond(defaultTimeTickInSeconds);
        // LogTime();
    }

    static void IncreaseCurrentSecond(int seconds)
    {
        currentSecond += seconds;
        if (currentSecond >= 60)
        {
            int minuteProgress = Mathf.FloorToInt(currentSecond / 60);
            IncreaseCurrentMinute(minuteProgress);
            currentSecond = currentSecond % 60;
        }
    }

    static void IncreaseCurrentMinute(int minutes)
    {
        currentMinute += minutes;
        if (currentMinute >= 60)
        {
            int hourProgress = Mathf.FloorToInt(currentMinute / 60);
            IncreaseCurrentHour(hourProgress);
            currentMinute = currentMinute % 60;
        }
    }

    static void IncreaseCurrentHour(int hours)
    {
        currentHour += hours;
        if (currentHour >= 24)
        {
            int dayProgress = Mathf.FloorToInt(currentHour / 24);
            IncreaseCurrentDay(dayProgress);
            currentHour = currentHour % 24;
        }
    }

    static void IncreaseCurrentDay(int days)
    {
        Debug.Log("It's a new day!");
    }

    public static Vector3 GetCurrentTime()
    {
        return new Vector3(currentHour, currentMinute, currentSecond);
    }

    public static int GetTotalSeconds(Vector3Int timeAmount)
    {
        return (timeAmount.x * 60 * 60) + (timeAmount.y * 60) + timeAmount.z;
    }

    public static void UpdateTimeDisplay()
    {
        // TODO
    }

    public static void LogTime()
    {
        Debug.Log(currentHour + ":" + currentMinute + ":" + currentSecond);
    }
}
