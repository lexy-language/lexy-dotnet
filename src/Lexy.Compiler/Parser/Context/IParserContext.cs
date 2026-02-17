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
    ISymbols Symbols { get; }

    ComponentNodeList Nodes { get; }
    LexyScriptNode RootNode { get; }

    ILineFilter LineFilter { get; }
    IProject Project { get; }

    void AddFileIncluded(IFile file);
    bool IsFileIncluded(IFile file);
}
