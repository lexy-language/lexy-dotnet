using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface IParserContext
{
    ILibraries Libraries { get; }
    IParserLogger Logger { get; }

    IFileSystem FileSystem { get; }

    ComponentNodeList Nodes { get; }
    LexyScriptNode RootNode { get; }

    ILineFilter LineFilter { get; }

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}
