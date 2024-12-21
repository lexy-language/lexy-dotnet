using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Compiler
{
    public class CompilerContext : ICompilerContext
    {
        public ExecutionEnvironment ExecutionEnvironment { get; }
        public ILogger<CompilerContext> Logger { get; }

        public CompilerContext(ILogger<CompilerContext> logger)
        {
            this.Logger = logger;
        }
    }

    public interface ICompilerContext
    {
        ExecutionEnvironment ExecutionEnvironment { get; }
        ILogger<CompilerContext> Logger { get; }
    }
}