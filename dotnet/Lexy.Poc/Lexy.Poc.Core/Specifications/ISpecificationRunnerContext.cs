using System.Collections.Generic;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Specifications
{
    public interface ISpecificationRunnerContext
    {
        int Failed { get; }
        IReadOnlyCollection<ISpecificationFileRunner> FileRunners { get; }

        IList<string> DebugMessages { get; }
        IList<string> Messages { get; }

        void Fail(Scenario scenario, string message);
        void LogGlobal(string message);
        void Log(string message);
        void Success(Scenario scenario);
        void LogDebug(string message);
        void Add(ISpecificationFileRunner fileRunner);
        IEnumerable<IScenarioRunner> FailedScenariosRunners();
        int CountScenarios();
    }
}