using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

internal class LookupByFunction : TableFunction
{
    public const string FunctionHelpValue =
        "Arguments: lookUpBy(Table, discriminatorValue, lookUpValue, Table.DiscriminatorValueColumn, Table.SearchValueColumn, Table.ResultColumn)";
    public const string Name = "lookUpBy";

    private const int Arguments = 6;
    private const int ArgumentTable = 0;
    private const int ArgumentLDiscriminatorValue = 1;
    private const int ArgumentLookupValue = 2;
    private const int ArgumentDiscriminatorValueColumn = 3;
    private const int ArgumentSearchValueColumn = 4;
    private const int ArgumentResultColumn = 5;

    public Expression DiscriminatorExpression { get; }
    public Expression ValueExpression { get; }

    public MemberAccessLiteral DiscriminatorValueColumn { get; }
    public MemberAccessLiteral SearchValueColumn { get; }
    public MemberAccessLiteral ResultColumn { get; }

    public VariableType SearchValueColumnType { get; private set; }
    public VariableType DiscriminatorValueColumnType { get; private set; }
    public VariableType ResultColumnType { get; private set; }

    public override string FunctionHelp => FunctionHelpValue;

    private LookupByFunction(string tableType, Expression discriminatorExpression, Expression valueExpression,
        MemberAccessLiteral discriminatorColumn, MemberAccessLiteral searchValueColumn, MemberAccessLiteral resultColumn,
        ExpressionSource source)
        : base(tableType, Name, source)
    {
        ValueExpression = valueExpression ?? throw new ArgumentNullException(nameof(valueExpression));
        DiscriminatorExpression = discriminatorExpression ?? throw new ArgumentNullException(nameof(discriminatorExpression));
        SearchValueColumn = searchValueColumn ?? throw new ArgumentNullException(nameof(searchValueColumn));
        DiscriminatorValueColumn = discriminatorColumn ?? throw new ArgumentNullException(nameof(discriminatorColumn));
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

        if (arguments[ArgumentResultColumn] is not MemberAccessExpression resultColumnExpression)
        {
            return ParseExpressionFunctionsResult.Failed(
                $"Invalid argument {ArgumentResultColumn}. Should be result column. {FunctionHelpValue}");
        }

        var tableName = tableNameExpression.Identifier;
        var discriminatorExpression = arguments[ArgumentLDiscriminatorValue];
        var valueExpression = arguments[ArgumentLookupValue];
        var discriminatorValueColumn = discriminatorValueColumnHeader.MemberAccessLiteral;
        var searchValueColumn = searchValueColumnHeader.MemberAccessLiteral;
        var resultColumn = resultColumnExpression.MemberAccessLiteral;

        var lookupFunction = new LookupByFunction(tableName, discriminatorExpression, valueExpression,
            discriminatorValueColumn, searchValueColumn, resultColumn, source);
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
        var resultColumnHeader = GetColumnHeader(context, ArgumentResultColumn, ResultColumn);
        if (discriminatorColumnHeader == null || searchColumnHeader == null || resultColumnHeader == null) return;

        ResultColumnType = resultColumnHeader.Type.VariableType;
        SearchValueColumnType = searchColumnHeader.Type.VariableType;
        DiscriminatorValueColumnType = discriminatorColumnHeader.Type.VariableType;

        ValidateColumnValueType(
            ArgumentSearchValueColumn, SearchValueColumn,
            ValueExpression.DeriveType(context), SearchValueColumnType, context);
        ValidateColumnValueType(
            ArgumentDiscriminatorValueColumn, DiscriminatorValueColumn,
            DiscriminatorExpression.DeriveType(context), DiscriminatorValueColumnType, context);
    }

    public override VariableType DeriveType(IValidationContext context) => ResultColumnType;
}