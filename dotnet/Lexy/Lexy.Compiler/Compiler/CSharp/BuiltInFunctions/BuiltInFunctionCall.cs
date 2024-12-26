using System;
using Lexy.Poc.Core.Language.Expressions;
using Lexy.Poc.Core.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Poc.Core.Compiler.CSharp.BuiltInFunctions
{
    internal abstract class BuiltInFunctionCall
    {
        public BuiltInFunction BuiltInFunction { get; }

        protected BuiltInFunctionCall(BuiltInFunction builtInFunction)
        {
            BuiltInFunction = builtInFunction ?? throw new ArgumentNullException(nameof(builtInFunction));
        }

        public static BuiltInFunctionCall Create(FunctionCallExpression expression)
        {
            return expression.BuiltInFunction switch
            {
                LookupFunction function => new LookUpFunctionCall(function),

                IntFunction function => new IntFunctionCall(function),
                AbsFunction function => new AbsFunctionCall(function),
                PowerFunction function => new PowerFunctionCall(function),
                RoundFunction function => new RoundFunctionCall(function),

                NowFunction function => new NowFunctionCall(function),
                TodayFunction function => new TodayFunctionCall(function),

                YearFunction function => new YearFunctionCall(function),
                MonthFunction function => new MonthFunctionCall(function),
                DayFunction function => new DayFunctionCall(function),
                HourFunction function => new HourFunctionCall(function),
                MinuteFunction function => new MinuteFunctionCall(function),
                SecondFunction function => new SecondFunctionCall(function),

                YearsFunction function => new YearsFunctionCall(function),
                MonthsFunction function => new MonthsFunctionCall(function),
                DaysFunction function => new DaysFunctionCall(function),
                HoursFunction function => new HoursFunctionCall(function),
                MinutesFunction function => new MinutesFunctionCall(function),
                SecondsFunction function => new SecondsFunctionCall(function),

                _ => throw new InvalidOperationException($"Invalid built in function call: {expression.FunctionName}")
            };
        }

        public abstract MemberDeclarationSyntax CustomMethodSyntax(ICompileFunctionContext context);

        public abstract ExpressionSyntax CallExpressionSyntax(ICompileFunctionContext context);
    }
}