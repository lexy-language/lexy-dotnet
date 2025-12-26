using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

internal class LookupRowFunction : TableFunction
{
    private const string FunctionHelpValue = " Arguments: lookUpRow(Table, lookUpValue, Table.SearchValueColumn)";

    public const string Name = "lookUpRow";

    private const int Arguments = 3;
    private const int ArgumentTable = 0;
    private const int ArgumentLookupValue = 1;
    private const int ArgumentSearchValueColumn = 2;

    public Expression ValueExpression { get; }

    public MemberAccessLiteral SearchValueColumn { get; }

    public VariableType SearchValueColumnType { get; private set; }
    public VariableType RowType { get; private set; }

    public override string FunctionHelp => FunctionHelpValue;

    private LookupRowFunction(string tableType, Expression valueExpression,
        MemberAccessLiteral searchValueColumn, ExpressionSource source)
        : base(tableType, Name, source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        SearchValueColumn = searchValueColumn ?? throw new ArgumentNullException(nameof(searchValueColumn));
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

        var tableName = tableNameExpression.Identifier;
        var valueExpression = arguments[ArgumentLookupValue];
        var searchValueColumn = searchValueColumnHeader.MemberAccessLiteral;

        var lookupFunction = new LookupRowFunction(tableName, valueExpression, searchValueColumn, source);
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
        if (searchColumnHeader == null) return;

        SearchValueColumnType = searchColumnHeader.Type.VariableType;

        ValidateColumnValueType(
            ArgumentSearchValueColumn, SearchValueColumn,
            ValueExpression.DeriveType(context), SearchValueColumnType, context);

        RowType = Table?.GetRowType();
    }

    public override VariableType DeriveType(IValidationContext context) => RowType;
}