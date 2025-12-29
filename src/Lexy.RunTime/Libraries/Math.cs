namespace Lexy.RunTime.Libraries;

public static class Math
{
    public static decimal Power(decimal number, decimal power)
    {
        return (decimal)System.Math.Pow((double)number, (double)power);
    }

    public static decimal Round(decimal number, decimal digits)
    {
        return System.Math.Round(number, (int)digits);
    }
}