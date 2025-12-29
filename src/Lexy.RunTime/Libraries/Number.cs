using System;

namespace Lexy.RunTime.Libraries;

public static class Number
{
    public static decimal Parse(string value)
    {
        return int.Parse(value);
    }

    public static decimal Floor(decimal value)
    {
        return System.Math.Floor(value);
    }

    public static decimal Abs(decimal value)
    {
        return System.Math.Abs(value);
    }
}