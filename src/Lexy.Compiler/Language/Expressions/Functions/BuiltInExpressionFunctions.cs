using System;
using System.Collections.Generic;

namespace Lexy.Compiler.Language.Expressions.Functions;

public static class BuiltInExpressionFunctions
{
    private static readonly
        IDictionary<string, Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult>>
        Values =
            new Dictionary<string,
                Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult>>
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

                { LookupFunction.Name, LookupFunction.Create },
                { LookupRowFunction.Name, LookupRowFunction.Create },
                { LookupByFunction.Name, LookupByFunction.Create },
                { LookupRowByFunction.Name, LookupRowByFunction.Create },

                { NewFunction.Name, Create(NewFunction.Create) },
                { FillParametersFunction.Name, Create(FillParametersFunction.Create) },
                { ExtractResultsFunction.Name, Create(ExtractResultsFunction.Create) }
            };

    public static ParseExpressionFunctionsResult Parse(string functionName, ExpressionSource source,
        IReadOnlyList<Expression> arguments)
    {
        return Values.TryGetValue(functionName, out var value)
            ? value(source, arguments)
            : ParseExpressionFunctionsResult.Success(new LexyFunction(functionName, arguments, source));
    }

    private static Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult> Create(
        Func<ExpressionSource, FunctionCallExpression> factory)
    {
        return (reference, arguments) =>
        {
            if (arguments.Count != 0)
                return ParseExpressionFunctionsResult.Failed("Invalid number of arguments. No arguments expected.");

            var function = factory(reference);
            return ParseExpressionFunctionsResult.Success(function);
        };
    }

    private static Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult> Create(
        Func<ExpressionSource, Expression, FunctionCallExpression> factory)
    {
        return (reference, arguments) =>
        {
            if (arguments.Count != 1)
                return ParseExpressionFunctionsResult.Failed("Invalid number of arguments. 1 argument expected.");

            var function = factory(reference, arguments[0]);
            return ParseExpressionFunctionsResult.Success(function);
        };
    }

    private static Func<ExpressionSource, IReadOnlyList<Expression>, ParseExpressionFunctionsResult> Create(
        Func<ExpressionSource, Expression, Expression, FunctionCallExpression> factory)
    {
        return (reference, arguments) =>
        {
            if (arguments.Count != 2)
                return ParseExpressionFunctionsResult.Failed("Invalid number of arguments. 2 arguments expected.");

            var function = factory(reference, arguments[0], arguments[1]);
            return ParseExpressionFunctionsResult.Success(function);
        };
    }
}