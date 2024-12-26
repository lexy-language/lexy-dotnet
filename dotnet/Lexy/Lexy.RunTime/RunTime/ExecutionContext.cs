
using System;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.RunTime
{
    public class ExecutionContext : IExecutionContext
    {
        private readonly ILogger<ExecutionContext> logger;

        public ExecutionContext(ILogger<ExecutionContext> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void LogDebug(string message)
        {
            logger.LogDebug(message);
        }

        public void LogVariable<T>(string name, T value)
        {
            logger.LogDebug("  {Name}: {Value}", name, value);
        }
    }
}