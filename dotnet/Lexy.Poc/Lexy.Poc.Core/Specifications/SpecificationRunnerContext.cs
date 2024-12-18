using System.Collections.Generic;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationRunnerContext
    {
        public IList<string> DebugMessages { get; } = new List<string>();
        public IList<string> Messages { get; } = new List<string>();

        public int Failed { get; private set; }

        public void Fail(Scenario scenario, string message)
        {
            Failed++;
            Messages.Add($"- FAILED  - {scenario.Name}: " + message);
        }

        public void LogGlobal(string message)
        {
            Messages.Add($"{message}");
        }

        public void Log(string message)
        {
            Messages.Add($"  {message}");
        }

        public void Success(Scenario scenario)
        {
            Messages.Add($"- SUCCESS - {scenario.Name}");
        }

        public void LogDebug(string message)
        {
            DebugMessages.Add(message);
        }
    }
}