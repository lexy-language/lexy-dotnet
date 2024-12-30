using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserContext
{
    IParserLogger Logger { get; }

    Line CurrentLine { get; }

    RootNodeList Nodes { get; }
    SourceCodeNode RootNode { get; }

    bool ProcessLine();

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}