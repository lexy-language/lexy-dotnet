using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

internal class LookupFunction : TableFunction
{
    public const string FunctionHelpValue =
        "Arguments: lookUp(Table, lookUpValue, Table.SearchValueColumn, Table.ResultColumn)";
    public const string Name = "lookUp";

    private const int Arguments = 4;
    private const int ArgumentTable = 0;
    private const int ArgumentLookupValue = 1;
    private const int ArgumentSearchValueColumn = 2;
    private const int ArgumentResultColumn = 3;

    public Expression ValueExpression { get; }

    public MemberAccessLiteral SearchValueColumn { get; }
    public MemberAccessLiteral ResultColumn { get; }

    public VariableType SearchValueColumnType { get; private set; }
    public VariableType ResultColumnType { get; private set; }

    public override string FunctionHelp => FunctionHelpValue;

    private LookupFunction(string tableType, Expression valueExpression,
        MemberAccessLiteral searchValueColumn, MemberAccessLiteral resultColumn,
        ExpressionSource source)
        : base(tableType, Name, source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        SearchValueColumn = searchValueColumn ?? throw new ArgumentNullException(nameof(searchValueColumn));
        ResultColumn = resultColumn ?? throw new ArgumentNullException(nameof(resultColumn));
    }

    public static ParseExpressionFunctionsResult Create(ExpressionSource source,
        IReadOnlyList<Expression> arguments)
    {
        if (arguments.Count != Arguments)
        {
            return ParseExpressionFunctionsResult.Failed($"Invalid number of arguments. {FunctionHelpValue}");
        }

        if (arguments[ArgumentTable] is not IdentifierExpression tableNameExpression)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentTable}. Should be valid table name. {FunctionHelpValue}");
        }

        if (arguments[ArgumentSearchValueColumn] is not MemberAccessExpression searchValueColumnHeader)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentSearchValueColumn}. Should be search column. {FunctionHelpValue}");
        }

        if (arguments[ArgumentResultColumn] is not MemberAccessExpression resultColumnExpression)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentResultColumn}. Should be result column. {FunctionHelpValue}");
        }

        var tableName = tableNameExpression.Identifier;
        var valueExpression = arguments[ArgumentLookupValue];
        var searchValueColumn = searchValueColumnHeader.MemberAccessLiteral;
        var resultColumn = resultColumnExpression.MemberAccessLiteral;

        var lookupFunction = new LookupFunction(tableName, valueExpression,
             searchValueColumn, resultColumn, source);
        return ParseExpressionFunctionsResult.Success(lookupFunction);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        base.Validate(context);
        if (Table == null) return;

        var searchColumnHeader = GetColumnHeader(context, ArgumentSearchValueColumn, SearchValueColumn);
        var resultColumnHeader = GetColumnHeader(context, ArgumentResultColumn, ResultColumn);
        if (searchColumnHeader == null || resultColumnHeader == null) return;

        ResultColumnType = resultColumnHeader.Type.VariableType;
        SearchValueColumnType = searchColumnHeader.Type.VariableType;

        ValidateColumnValueType(
            ArgumentSearchValueColumn, SearchValueColumn,
            ValueExpression.DeriveType(context), SearchValueColumnType, context);
    }

    public override VariableType DeriveType(IValidationContext context) => ResultColumnType;
}