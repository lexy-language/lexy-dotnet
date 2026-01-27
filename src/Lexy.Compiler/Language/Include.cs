using System.Threading.Tasks;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public class Include
{
    private readonly SourceReference reference;

    public bool IsProcessed { get; private set; }
    public string FileName { get; }

    private Include(string fileName, SourceReference reference)
    {
        this.reference = reference;
        FileName = Assert.NotNull(fileName, nameof(fileName));
    }

    public static bool IsValid(Line line)
    {
        return line.Tokens.IsKeyword(0, Keywords.Include);
    }

    public static Include Parse(IParseLineContext context)
    {
        var line = context.Line;
        var lineTokens = line.Tokens;
        if (lineTokens.Length != 2 || !lineTokens.IsQuotedString(1))
        {
            context.Logger.Fail(lineTokens.AllReference(),
                "Invalid syntax. Expected: 'include \"FileName\"");
            return null;
        }

        var quotedString = lineTokens.Token<QuotedLiteralToken>(1);

        return new Include(quotedString.Value, lineTokens.AllReference());
    }

    public async Task<string> Process(string parentFullFileName, IParserContext context)
    {
        IsProcessed = true;
        if (string.IsNullOrEmpty(FileName))
        {
            context.Logger.Fail(reference, "No include file name specified.");
            return null;
        }

        var directName = context.FileSystem.GetDirectoryName(parentFullFileName);
        var fullPath = context.FileSystem.GetFullPath(directName);
        var fullFileName = $"{context.FileSystem.Combine(fullPath, FileName)}.{LexySourceDocument.FileExtension}";

        if (!await context.FileSystem.FileExists(fullFileName))
        {
            context.Logger.Fail(reference, $"Invalid include file name '{FileName}'");
            return null;
        }

        return fullFileName;
    }
}
