using System;

namespace Lexy.Compiler.Infrastructure;

public static class GlobalTiming
{
    private static DateTime start;
    private static DateTime last;

    public static void Init()
    {
        start = last = DateTime.Now;
    }

    public static void Log(string message)
    {
        var totalMilliseconds = (DateTime.Now - last).TotalMilliseconds;
        if (totalMilliseconds > 1)
        {
            Console.WriteLine(
                $"{message}: {(DateTime.Now - start).TotalSeconds} ({totalMilliseconds}) {(totalMilliseconds > 1 ? "!!!!" : "")}");
        }

        last = DateTime.Now;
    }
}
