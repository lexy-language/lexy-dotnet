using System;

namespace Lexy.Poc.Core.RunTime
{
    public static class BuiltInDateFunctions
    {
        public static DateTime Now() => DateTime.Now;
        public static DateTime Today() => DateTime.Today;

        public static decimal Year(DateTime value) => value.Year;
        public static decimal Month(DateTime value) => value.Month;
        public static decimal Day(DateTime value) => value.Day;
        public static decimal Hour(DateTime value) => value.Hour;
        public static decimal Minute(DateTime value) => value.Minute;
        public static decimal Second(DateTime value) => value.Second;

        public static decimal Years(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Years;
        public static decimal Months(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Months;
        public static decimal Days(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Days;
        public static decimal Hours(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Hours;
        public static decimal Minutes(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Minutes;
        public static decimal Seconds(DateTime end, DateTime start) => DateTimeSpan.CompareDates(end, start).Seconds;
    }
}