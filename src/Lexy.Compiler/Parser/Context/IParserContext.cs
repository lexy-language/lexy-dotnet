using Lexy.Compiler.FunctionLibraries;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;

namespace Lexy.Compiler.Parser.Context;

public interface IParserContext
{
    ILibraries Libraries { get; }
    IParserLogger Logger { get; }

    IFileSystem FileSystem { get; }
    DocumentsSymbols Symbols { get; }

    ComponentNodeList Nodes { get; }
    LexyScriptNode RootNode { get; }

    ILineFilter LineFilter { get; }

    void AddFileIncluded(string fileName);
    bool IsFileIncluded(string fileName);
}
