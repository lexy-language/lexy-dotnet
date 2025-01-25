using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal static class FunctionCallFactory
{
    private static IDictionary<Type, Func<ExpressionFunction, ExpressionSyntax>> expressionSyntaxConvertors
        = new Dictionary<Type, Func<ExpressionFunction, ExpressionSyntax>>();
    private static IDictionary<Type, Func<ExpressionFunction, MemberDeclarationSyntax>> customMethods
        = new Dictionary<Type, Func<ExpressionFunction, MemberDeclarationSyntax>>();

    static FunctionCallFactory()
    {
        AddFactory<LookupFunction, LookUpFunctionCall>();
        AddFactory<LookupRowFunction, LookUpRowFunctionCall>();

        AddFactory<IntFunction, IntFunctionCall>();
        AddFactory<AbsFunction, AbsFunctionCall>();
        AddFactory<PowerFunction, PowerFunctionCall>();
        AddFactory<RoundFunction, RoundFunctionCall>();

        AddFactory<NowFunction, NowFunctionCall>();
        AddFactory<TodayFunction, TodayFunctionCall>();

        AddFactory<YearFunction, YearFunctionCall>();
        AddFactory<MonthFunction, MonthFunctionCall>();
        AddFactory<DayFunction, DayFunctionCall>();
        AddFactory<HourFunction, HourFunctionCall>();
        AddFactory<MinuteFunction, MinuteFunctionCall>();
        AddFactory<SecondFunction, SecondFunctionCall>();

        AddFactory<YearsFunction, YearsFunctionCall>();
        AddFactory<MonthsFunction, MonthsFunctionCall>();
        AddFactory<DaysFunction, DaysFunctionCall>();
        AddFactory<HoursFunction, HoursFunctionCall>();
        AddFactory<MinutesFunction, MinutesFunctionCall>();
        AddFactory<SecondsFunction, SecondsFunctionCall>();

        AddFactory<LexyFunction, LexyFunctionCall>();
    }

    private static void AddFactory<TFunction, TFunctionCall>()
        where TFunctionCall : new()
        where TFunction : ExpressionFunction
    {
        var callExpressionSyntaxMethod = typeof(TFunctionCall)
            .GetMethod(nameof(FunctionCall<TFunction>.CallExpressionSyntax), BindingFlags.Instance | BindingFlags.Public);

        var customMethodSyntaxMethod = typeof(TFunctionCall)
            .GetMethod(nameof(FunctionCall<TFunction>.CustomMethodSyntax), BindingFlags.Instance | BindingFlags.Public);

        expressionSyntaxConvertors.Add(typeof(TFunction), expression =>
        {
            var functionCall = new TFunctionCall();
            if (expression is not TFunction specific)
            {
                throw new InvalidOperationException($"Invalid function type: '{expression.GetType()}' expected '{typeof(TFunction)}'");
            }
            return (ExpressionSyntax) callExpressionSyntaxMethod.Invoke(functionCall, new []{ specific });
        });

        customMethods.Add(typeof(TFunction), expression =>
        {
            var functionCall = new TFunctionCall();
            if (expression is not TFunction specific)
            {
                throw new InvalidOperationException($"Invalid function type: '{expression.GetType()}' expected '{typeof(TFunction)}'");
            }
            return (MemberDeclarationSyntax) customMethodSyntaxMethod.Invoke(functionCall, new []{ specific });
        });
    }

    public static ExpressionSyntax CallExpressionSyntax(ExpressionFunction expressionExpressionFunction)
    {
        if (expressionExpressionFunction == null) throw new ArgumentNullException(nameof(expressionExpressionFunction));

        if (!expressionSyntaxConvertors.TryGetValue(expressionExpressionFunction.GetType(), out var functionCall))
        {
            throw new InvalidOperationException($"Invalid function type: '{expressionExpressionFunction.GetType()}'. No function call found.");

        }
        return functionCall(expressionExpressionFunction);
    }

    public static MemberDeclarationSyntax CustomMethods(ExpressionFunction expressionExpressionFunction)
    {
        if (expressionExpressionFunction == null) throw new ArgumentNullException(nameof(expressionExpressionFunction));

        return customMethods.TryGetValue(expressionExpressionFunction.GetType(), out var functionCall)
            ? functionCall(expressionExpressionFunction)
            : null;
    }
}