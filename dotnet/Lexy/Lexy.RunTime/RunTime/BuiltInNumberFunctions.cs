using System;

namespace Lexy.Poc.Core.RunTime
{
    public static class BuiltInNumberFunctions
    {
        public static decimal Int(decimal value) => Math.Floor(value);
        public static decimal Abs(decimal value) => Math.Abs(value);
        public static decimal Power(decimal number, decimal power) => (decimal)Math.Pow((double) number, (double) power);
        public static decimal Round(decimal number, decimal digits) => Math.Round(number, (int) digits);
    }
}