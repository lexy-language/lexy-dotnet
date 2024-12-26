using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language.Expressions.Functions
{
    public static class BuiltInFunctions
    {
        private static readonly IDictionary<string, Func<string, SourceReference, IReadOnlyList<Expression>, ParseBuiltInFunctionsResult>> Values =
            new Dictionary<string, Func<string, SourceReference, IReadOnlyList<Expression>, ParseBuiltInFunctionsResult>>
        {
            { IntFunction.Name, Create(IntFunction.Create) },
            { AbsFunction.Name, Create(AbsFunction.Create) },
            { PowerFunction.Name, Create(PowerFunction.Create) },
            { RoundFunction.Name, Create(RoundFunction.Create) },

            { NowFunction.Name, Create(NowFunction.Create) },
            { TodayFunction.Name, Create(TodayFunction.Create) },

            { YearFunction.Name, Create(YearFunction.Create) },
            { MonthFunction.Name, Create(MonthFunction.Create) },
            { DayFunction.Name, Create(DayFunction.Create) },
            { HourFunction.Name, Create(HourFunction.Create) },
            { MinuteFunction.Name, Create(MinuteFunction.Create) },
            { SecondFunction.Name, Create(SecondFunction.Create) },

            { YearsFunction.Name, Create(YearsFunction.Create) },
            { MonthsFunction.Name, Create(MonthsFunction.Create) },
            { DaysFunction.Name, Create(DaysFunction.Create) },
            { HoursFunction.Name, Create(HoursFunction.Create) },
            { MinutesFunction.Name, Create(MinutesFunction.Create) },
            { SecondsFunction.Name, Create(SecondsFunction.Create) },

            { LookupFunction.Name, LookupFunction.Parse },
        };

        public static ParseBuiltInFunctionsResult Parse(string functionName, SourceReference reference,
            IReadOnlyList<Expression> arguments)
        {
            return Values.TryGetValue(functionName, out var value)
                ? value(functionName, reference, arguments)
                : ParseBuiltInFunctionsResult.Failed($"Unknown function name: '{functionName}'");
        }

        private static Func<string, SourceReference, IReadOnlyList<Expression>, ParseBuiltInFunctionsResult> Create(Func<SourceReference, BuiltInFunction> factory) =>
            (name, reference, arguments) =>
            {
                if (arguments.Count != 0)
                {
                    return ParseBuiltInFunctionsResult.Failed($"Invalid number of arguments. No arguments expected.");
                }

                var function = factory(reference);
                return ParseBuiltInFunctionsResult.Success(function);
            };

        private static Func<string, SourceReference, IReadOnlyList<Expression>, ParseBuiltInFunctionsResult> Create(Func<SourceReference, Expression, BuiltInFunction> factory) =>
            (name, reference, arguments) =>
            {
                if (arguments.Count != 1)
                {
                    return ParseBuiltInFunctionsResult.Failed($"Invalid number of arguments. 1 argument expected.");
                }

                var function = factory(reference, arguments[0]);
                return ParseBuiltInFunctionsResult.Success(function);
            };

        private static Func<string, SourceReference, IReadOnlyList<Expression>, ParseBuiltInFunctionsResult> Create(Func<SourceReference, Expression, Expression, BuiltInFunction> factory) =>
            (name, reference, arguments) =>
            {
                if (arguments.Count != 2)
                {
                    return ParseBuiltInFunctionsResult.Failed($"Invalid number of arguments. 2 arguments expected.");
                }

                var function = factory(reference, arguments[0], arguments[1]);
                return ParseBuiltInFunctionsResult.Success(function);
            };
    }
}