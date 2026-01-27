namespace Lexy.Compiler.Parser.Documents;

public interface ISourceCodeDocument
{
    string FullFileName { get; }

    bool HasMoreLines();
    Line NextLine();
}
