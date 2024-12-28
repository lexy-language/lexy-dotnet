using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;

namespace Lexy.Compiler.Compiler;

public interface ILexyCompiler
{
    CompilerResult Compile(RootNodeList rootNodeList, Function function);
}