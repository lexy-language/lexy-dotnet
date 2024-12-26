using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public interface ICompilerContext
    {
        ILogger<CompilerContext> Logger { get; }
    }
}