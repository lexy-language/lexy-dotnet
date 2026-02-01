using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Tests.Symbols;

public static class VerifySymbolsExtensions
{
    public static VerifyModelContext<DocumentsSymbols> Description(this VerifyModelContext<DocumentsSymbols> symbols, int lineNumber, int column,
        string expectedName, SymbolKind expectedKind, string expectedDescription = null)
    {
        var extraMessage = $"Symbol at ({lineNumber}:{column})";
        var position = new Position(lineNumber, column);

        symbols.IsNotNull(model => model.GetDescription("test.lexy", position),_ => _
            .AreEqual(description => description.Name, expectedName, extraMessage)
            .AreEqual(description => description.Kind, expectedKind, extraMessage)
            .IfNotNull(expectedDescription, __ => __
                .AreEqual(description => description.Description, expectedDescription, extraMessage)
            ),
            extraMessage
        );

        return symbols;
    }

    public static VerifyModelContext<DocumentsSymbols> VerifyDescriptionNull(this VerifyModelContext<DocumentsSymbols> symbols, int lineNumber, params int[] columns)
    {
        foreach (var column in columns)
        {
            symbols.IsNull(model => model
                .GetDescription("test.lexy", new Position(lineNumber, column)), $"Symbol at {lineNumber}:{column}");
        }

        return symbols;
    }
}
