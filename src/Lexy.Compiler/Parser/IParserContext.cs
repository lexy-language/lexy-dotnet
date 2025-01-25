using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserContext
{
    IParserLogger Logger { get; }

    RootNodeList Nodes { get; }
    SourceCodeNode RootNode { get; }

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}