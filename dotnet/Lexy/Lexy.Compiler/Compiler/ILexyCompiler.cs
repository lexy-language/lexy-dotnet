using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Compiler
{
    public interface ILexyCompiler
    {
        CompilerResult Compile(Nodes nodes, Function function);
    }
}