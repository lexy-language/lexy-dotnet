using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal static class LookUpRowFunctionSymbol
{
    private const string Description = @"The `LookUpRow`function returns a specific row from a table.

The function will loop over all rows in a table from the start and will compare the value of a specific column `searchValueColumn` with the defined `lookUpValue`.
- If the value in the column equals the `lookUpValue`, the row is returned.
- If the value in the column exceeds the `lookUpValue`, the value `resultColumn` of the previous row is returned.

NOTE: table search value columns should be sorted from small to large in order these functions to work correctly. This also applies to string columns, they should be sorted alphabetically.";

    public static Symbol Create(SourceReference reference, string tableName, Type resultsType)
    {
        return SymbolBuilder.Build(build => build
            .Reference(reference)
            .Name($"table function: {tableName}.LookUpRow")
            .Description(Description)
            .Kind(SymbolKind.TableFunction)
            .Signatures(signatures => signatures
                .Signature($"LookUpRow from table. Search column is the first column. Result type '{resultsType}'.", signature => signature
                    .Parameter("lookUpValue", "The value to search for")
                )
                .Signature($"LookUpRow from table. Result type '{resultsType}'.", signature => signature
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.SearchColumn", "The column to find the search value in")
                )
                .Signature(
                    $"LookUpRow value from result column and a discriminator. The discriminator column is the first column, the search column is the first column. Result type '{resultsType}'.", signature => signature
                    .Parameter("discriminator", "The discriminator value")
                    .Parameter("lookUpValue", "The value to search for")
                )
                .Signature($"LookUpRow value from result column and a discriminator. Result type '{resultsType}'.", signature => signature
                    .Parameter("discriminator", "The discriminator value")
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.DiscriminatorColumn", "The discriminator column")
                    .Parameter("Table.SearchColumn", "The column to find the search value in")
                )
            ));
    }
}
