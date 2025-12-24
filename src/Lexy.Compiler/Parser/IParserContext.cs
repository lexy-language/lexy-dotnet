using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserContext
{
    IParserLogger Logger { get; }

    ComponentNodeList Nodes { get; }
    SourceCodeNode RootNode { get; }

    ILineFilter LineFilter { get; }

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}