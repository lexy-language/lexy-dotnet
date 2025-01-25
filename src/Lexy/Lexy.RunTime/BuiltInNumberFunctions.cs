using System;

namespace Lexy.RunTime;

public static class BuiltInNumberFunctions
{
    public static decimal Int(decimal value)
    {
        return Math.Floor(value);
    }

    public static decimal Abs(decimal value)
    {
        return Math.Abs(value);
    }

    public static decimal Power(decimal number, decimal power)
    {
        return (decimal)Math.Pow((double)number, (double)power);
    }

    public static decimal Round(decimal number, decimal digits)
    {
        return Math.Round(number, (int)digits);
    }
}