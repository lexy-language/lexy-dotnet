using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler
{
    public interface ILexyCompiler
    {
        CompilerResult Compile(RootNodeList rootNodeList, Function function);
    }
}