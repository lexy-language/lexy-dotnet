using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.VariableTypes.Functions;

internal class LookUpFunction : TableFunctionReference
{
    private const string FunctionHelpValue =
        "Arguments: " +
           "Table.LookUp(lookUpValue, Table.ResultColumn) " +
        "or Table.LookUp(lookUpValue, Table.SearchColumn, Table.ResultColumn)" +
        "or Table.LookUp(discriminator, lookUpValue, Table.ResultColumn)" +
        "or Table.LookUp(discriminator, lookUpValue, Table.DiscriminatorColumn, Table.SearchColumn, Table.ResultColumn)";

    private record OverloadArguments(
        int? Discriminator,
        int LookUpValue,
        int? DiscriminatorColumnArgument,
        int? DefaultDiscriminatorColumn,
        int? SearchColumnArgument,
        int DefaultSearchColumn,
        int ResultColumnArgument);

    protected override string FunctionHelp => FunctionHelpValue;

    public const string Name = "LookUp";

    internal LookUpFunction(Table table): base(table)
    {
    }

    public override VariableType GetResultsType(IReadOnlyList<Expression> arguments)
    {
        var overloadArguments = GetArgumentColumns(null, arguments, null);

        var argument = arguments[overloadArguments.ResultColumnArgument];
        if (argument is not MemberAccessExpression columnExpression)
        {
            return null;
        }

        var column = Table.Header?.Get(columnExpression.VariablePath);
        return column?.Type.VariableType;
    }

    public override ValidateInstanceFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments,
        SourceReference reference)
    {
        if (!ValidateTable(context, reference)) return ValidateInstanceFunctionArgumentsResult.Failed();

        var overloadArguments = GetArgumentColumns(context, arguments, reference);
        var searchColumnHeader = GetColumn(context, arguments, overloadArguments.SearchColumnArgument, overloadArguments.DefaultSearchColumn, reference) ;
        var resultColumnHeader = GetColumn(context, arguments, overloadArguments.ResultColumnArgument, null, reference) ;

        if (!ValidateArguments(overloadArguments, searchColumnHeader, resultColumnHeader)) return ValidateInstanceFunctionArgumentsResult.Failed();

        ValidateColumnValueType(context, arguments, overloadArguments.LookUpValue, "Search", searchColumnHeader, reference);

        var discriminatorColumnHeader = ValidateDiscriminator(context, arguments, reference, overloadArguments);

        var result = new LookUpFunctionCall(
            Table.Name.Value,
            arguments[overloadArguments.LookUpValue],
            overloadArguments.Discriminator.HasValue ? arguments[overloadArguments.Discriminator.Value] : null,
            resultColumnHeader.Name,
            searchColumnHeader.Name,
            discriminatorColumnHeader?.Name);

        return ValidateInstanceFunctionArgumentsResult.Success(result);
    }

    private bool ValidateArguments(OverloadArguments overloadArguments, ColumnHeader searchColumnHeader, ColumnHeader resultColumnHeader)
    {
        if (searchColumnHeader == null || resultColumnHeader == null) return false;


        return true;
    }

    private ColumnHeader ValidateDiscriminator(IValidationContext context, IReadOnlyList<Expression> arguments,
        SourceReference reference, OverloadArguments overloadArguments)
    {
        if (overloadArguments.Discriminator == null) return null;

        var discriminatorColumnHeader = overloadArguments.DefaultDiscriminatorColumn != null
            ? GetColumn(context, arguments, overloadArguments.DiscriminatorColumnArgument, overloadArguments.DefaultDiscriminatorColumn, reference)
            : null;
        ValidateColumnValueType(context, arguments, overloadArguments.Discriminator.Value, "Discriminator", discriminatorColumnHeader, reference);

        return discriminatorColumnHeader;
    }

    private OverloadArguments GetArgumentColumns(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference)
    {
        switch (arguments.Count)
        {
            case 2:
                //"Table.LookUp(lookUpValue, Table.ResultColumn) " +
                return new OverloadArguments(null, 0, null, null, null, 0, 1);

            case 3:
                //"Table.LookUp(lookUpValue, Table.SearchColumn, Table.ResultColumn)"
                if (arguments[1] is MemberAccessExpression)
                {
                    return new OverloadArguments(null, 0, null, null, 1, 0, 2);
                }
                //"Table.LookUp(discriminator, lookUpValue, Table.ResultColumn)"
                return new OverloadArguments(0, 1, null, 0, null, 1, 2);

            case 5:
                //"Table.LookUp(discriminator, lookUpValue, Table.DiscriminatorColumn, Table.SearchColumn, Table.ResultColumn)";
                return new OverloadArguments(0, 1, 2, 0, 3, 1, 4);

            default:
                context?.Logger.Fail(reference, $"Invalid number of arguments. {FunctionHelpValue}");
                return null;
        }
    }
}