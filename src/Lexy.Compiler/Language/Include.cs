using System.Threading.Tasks;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

public class Include
{
    private readonly SourceReference reference;

    public string FileName { get; }

    public IncludeState State { get; }

    private Include(string fileName, SourceReference reference)
    {
        this.reference = reference;
        FileName = Assert.NotNull(fileName, nameof(fileName));
        State = new IncludeState(false);
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

    public async Task<IFile> Process(IFile parentFile, IParserContext context)
    {
        State.SetProcessed();

        if (string.IsNullOrEmpty(FileName))
        {
            context.Logger.Fail(reference, "No include file name specified.");
            return null;
        }

        var directName = context.FileSystem.GetDirectoryName(parentFile.Name);
        var relativeFileName = $"{context.FileSystem.Combine(directName, FileName)}.{LexySourceDocument.FileExtension}";
        var file = context.Project.File(relativeFileName);

        if (!await context.FileSystem.FileExists(file.FullPath))
        {
            context.Logger.Fail(reference, $"Invalid include file name '{FileName}'");
            return null;
        }

        return file;
    }
}
