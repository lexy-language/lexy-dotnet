using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Tables;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal class LookUpRowFunction : TableFunction
{
    private const string FunctionHelpValue =
        "Arguments: " +
           "TableName.LookUpRow(lookUpValue) " +
        "or TableName.LookUpRow(lookUpValue, Table.SearchColumn) " +
        "or TableName.LookUpRow(discriminator, lookUpValue) " +
        "or TableName.LookUpRow(discriminator, lookUpValue, Table.DiscriminatorColumn, Table.SearchColumn)";

    private record OverloadArguments(
        int? Discriminator,
        int LookUpValue,
        int? DiscriminatorColumnArgument,
        int? DefaultDiscriminatorColumn,
        int? SearchColumnArgument,
        int DefaultSearchColumn) : IOverloadArguments;

    public const string Name = "LookUpRow";

    protected override string FunctionHelp => FunctionHelpValue;

    internal LookUpRowFunction(Table table): base(Name, table)
    {
    }

    public override Type GetResultsType(IReadOnlyList<Expression> arguments) => Table?.GetRowType();

    public override ValidateMemberFunctionArgumentsResult ValidateArguments(IValidationContext context, IReadOnlyList<Expression> arguments,
        SourceReference reference)
    {
         if (!ValidateTable(context, reference)) return ValidateMemberFunctionArgumentsResult.Failed();

        var overloadArguments = GetArgumentColumns(context, arguments, reference);
        if (overloadArguments == null) return null;

        var searchColumnHeader = GetColumn(context, arguments, overloadArguments.SearchColumnArgument, overloadArguments.DefaultSearchColumn, reference);

        if (searchColumnHeader == null) return ValidateMemberFunctionArgumentsResult.Failed();

        ValidateColumnValueType(context, arguments, overloadArguments.LookUpValue, "Search", searchColumnHeader, reference);

        var discriminatorColumnHeader = ValidateDiscriminator(context, arguments, reference, overloadArguments);

        var result = new LookUpRowFunctionCallState(
            reference,
            Table.Name,
            arguments[overloadArguments.LookUpValue],
            overloadArguments.Discriminator.HasValue ? arguments[overloadArguments.Discriminator.Value] : null,
            searchColumnHeader.Name,
            discriminatorColumnHeader?.Name,
            GetResultsType(arguments));

        return ValidateMemberFunctionArgumentsResult.Success(result);
    }

    private OverloadArguments GetArgumentColumns(IValidationContext context, IReadOnlyList<Expression> arguments, SourceReference reference)
    {
        switch (arguments.Count)
        {
            case 1:
                //"table.LookUpRow(lookUpValue)"
                return new OverloadArguments(null, 0, null, null, null, 0);

            case 2:
                //"table.LookUpRow(lookUpValue, Table.SearchColumn)"
                if (arguments[1] is MemberAccessExpression)
                {
                    return new OverloadArguments(null, 0, null, null, 1, 0);
                }
                //"table.LookUpRow(discriminator, lookUpValue)"
                return new OverloadArguments(0, 1, null, 0, null, 1);

            case 4:
                //"table.LookUpRow(discriminator, lookUpValue, Table.DiscriminatorColumn, Table.SearchColumn)";
                return new OverloadArguments(0, 1, 2, 0, 3, 1);

            default:
                context?.Logger.Fail(reference, $"Invalid number of arguments. {FunctionHelpValue}");
                return null;
        }
    }
}
