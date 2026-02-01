using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.TypeSystem.Functions;

internal static class LookUpFunctionSymbol
{
    const string Description = @"The `LookUp`function returns a specific value from the `resultColumn` column from a table.

The function will loop over all rows in a table from the start and will compare the value of a specific column `searchValueColumn` with the defined `lookUpValue`.
- If the value in the column equals the `lookUpValue`, the value in the `resultColumn` is returned.
- If the value in the column exceeds the `lookUpValue`, the value `resultColumn` of the previous row or the previous row is returned.

NOTE: table search value columns should be sorted from small to large in order these functions to work correctly. This also applies to string columns, they should be sorted alphabetically.";

    public static Symbol Create(SourceReference reference, string tableName, Type resultsType)
    {
        return SymbolBuilder.Build(build => build
            .Reference(reference)
            .Name($"table function: {tableName}.LookUp")
            .Description(Description)
            .Kind(SymbolKind.TableFunction)
            .Signatures(signatures => signatures
                .Signature($"Lookup value from result column. Search column is the first column. Result type '{resultsType}'.", signature => signature
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.ResultColumn", "The column to return the value from")
                )
                .Signature($"Lookup value from result column. Result type '{resultsType}'.", signature => signature
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.SearchColumn", "The column to find the search value in")
                    .Parameter("Table.ResultColumn", "The column to return the value from")
                )
                .Signature($"Lookup value from result column by discriminator. The discriminator column is the first column, the search column is the first column. Result type '{resultsType}'.", signature => signature
                    .Parameter("discriminator", "The discriminator value")
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.ResultColumn", "The column to return the value from")
                )
                .Signature($"Lookup value from result column by discriminator. Result type '{resultsType}'.", signature => signature
                    .Parameter("discriminator", "The discriminator value")
                    .Parameter("lookUpValue", "The value to search for")
                    .Parameter("Table.DiscriminatorColumn", "The discriminator column")
                    .Parameter("Table.SearchColumn", "The column to find the search value in")
                    .Parameter("Table.ResultColumn", "The column to return the value from")
                )
            ));
    }
}
