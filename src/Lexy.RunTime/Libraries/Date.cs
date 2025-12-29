using System;

namespace Lexy.RunTime.Libraries;

public static class Date
{
    public static string Format(DateTime value)
    {
        return value.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static DateTime Now()
    {
        return DateTime.Now;
    }

    public static DateTime Today()
    {
        return DateTime.Today;
    }

    public static decimal Year(DateTime value)
    {
        return value.Year;
    }

    public static decimal Month(DateTime value)
    {
        return value.Month;
    }

    public static decimal Day(DateTime value)
    {
        return value.Day;
    }

    public static decimal Hour(DateTime value)
    {
        return value.Hour;
    }

    public static decimal Minute(DateTime value)
    {
        return value.Minute;
    }

    public static decimal Second(DateTime value)
    {
        return value.Second;
    }

    public static decimal Millisecond(DateTime value)
    {
        return value.Millisecond;
    }

    public static decimal Years(DateTime end, DateTime start)
    {
        return DateTimeSpan.CompareDates(end, start).Years;
    }

    public static decimal Months(DateTime end, DateTime start)
    {
        return DateTimeSpan.CompareDates(end, start).Months;
    }

    public static decimal Days(DateTime end, DateTime start)
    {
        return DateTimeSpan.CompareDates(end, start).Days;
    }

    public static decimal Hours(DateTime end, DateTime start)
    {
        return (decimal) (end - start).TotalHours;
    }

    public static decimal Minutes(DateTime end, DateTime start)
    {
        return (decimal) (end - start).TotalMinutes;
    }

    public static decimal Seconds(DateTime end, DateTime start)
    {
        return (decimal) (end - start).TotalSeconds;
    }

    public static decimal Milliseconds(DateTime end, DateTime start)
    {
        return (decimal) (end - start).TotalMilliseconds;
    }
}