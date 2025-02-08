using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserContext
{
    IParserLogger Logger { get; }

    RootNodeList Nodes { get; }
    SourceCodeNode RootNode { get; }

    ParseOptions Options { get; }
    ILineFilter LineFilter { get; }

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}