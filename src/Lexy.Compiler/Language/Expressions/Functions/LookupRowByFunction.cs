using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

internal class LookupRowByFunction : TableFunction
{
    private const string FunctionHelpValue = " Arguments: lookUpRowBy(Table, discriminatorValue, lookUpValue, Table.DiscriminatorValueColumn, Table.SearchValueColumn)";
    public const string Name = "lookUpRowBy";

    private const int Arguments = 5;
    private const int ArgumentTable = 0;
    private const int ArgumentLDiscriminatorValue = 1;
    private const int ArgumentLookupValue = 2;
    private const int ArgumentDiscriminatorValueColumn = 3;
    private const int ArgumentSearchValueColumn = 4;

    public Expression DiscriminatorExpression { get; }
    public Expression ValueExpression { get; }

    public MemberAccessLiteral DiscriminatorValueColumn { get; }
    public MemberAccessLiteral SearchValueColumn { get; }

    public VariableType SearchValueColumnType { get; private set; }
    public VariableType DiscriminatorValueColumnType { get; private set; }

    public VariableType RowType { get; private set; }

    public override string FunctionHelp => FunctionHelpValue;

    private LookupRowByFunction(string tableType, Expression discriminatorExpression, Expression valueExpression,
        MemberAccessLiteral discriminatorColumn, MemberAccessLiteral searchValueColumn,
        ExpressionSource source)
        : base(tableType, Name, source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        DiscriminatorExpression = discriminatorExpression ?? throw new ArgumentNullException(nameof(discriminatorExpression));
        SearchValueColumn = searchValueColumn ?? throw new ArgumentNullException(nameof(searchValueColumn));
        DiscriminatorValueColumn = discriminatorColumn ?? throw new ArgumentNullException(nameof(discriminatorColumn));
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

        if (arguments[ArgumentDiscriminatorValueColumn] is not MemberAccessExpression discriminatorValueColumnHeader)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentDiscriminatorValueColumn}. Should be discriminator column. {FunctionHelpValue}");
        }

        if (arguments[ArgumentSearchValueColumn] is not MemberAccessExpression searchValueColumnHeader)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentSearchValueColumn}. Should be search column. {FunctionHelpValue}");
        }

        var tableName = tableNameExpression.Identifier;
        var discriminatorExpression = arguments[ArgumentLDiscriminatorValue];
        var valueExpression = arguments[ArgumentLookupValue];
        var discriminatorValueColumn = discriminatorValueColumnHeader.MemberAccessLiteral;
        var searchValueColumn = searchValueColumnHeader.MemberAccessLiteral;

        var lookupFunction = new LookupRowByFunction(tableName, discriminatorExpression, valueExpression,
            discriminatorValueColumn, searchValueColumn, source);
        return ParseExpressionFunctionsResult.Success(lookupFunction);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return ValueExpression;
        yield return DiscriminatorExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        base.Validate(context);
        if (Table == null) return;

        var discriminatorColumnHeader = GetColumnHeader(context, ArgumentDiscriminatorValueColumn, DiscriminatorValueColumn);
        var searchColumnHeader = GetColumnHeader(context, ArgumentSearchValueColumn, SearchValueColumn);
        if (discriminatorColumnHeader == null || searchColumnHeader == null) return;

        SearchValueColumnType = searchColumnHeader.Type.VariableType;
        DiscriminatorValueColumnType = discriminatorColumnHeader.Type.VariableType;

        ValidateColumnValueType(
            ArgumentSearchValueColumn, SearchValueColumn,
            ValueExpression.DeriveType(context), SearchValueColumnType, context);
        ValidateColumnValueType(
            ArgumentDiscriminatorValueColumn, DiscriminatorValueColumn,
            DiscriminatorExpression.DeriveType(context), DiscriminatorValueColumnType, context);

        RowType = Table?.GetRowType();
    }

    public override VariableType DeriveType(IValidationContext context) => RowType;
}