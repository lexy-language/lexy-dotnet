using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationRunnerContext : ISpecificationRunnerContext, IDisposable
    {
        private readonly List<ISpecificationFileRunner> fileRunners = new List<ISpecificationFileRunner>();

        public IList<string> DebugMessages { get; } = new List<string>();
        public IList<string> Messages { get; } = new List<string>();

        public int Failed { get; private set; }

        public IReadOnlyCollection<ISpecificationFileRunner> FileRunners => fileRunners;

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

        public void Add(ISpecificationFileRunner fileRunner)
        {
            fileRunners.Add(fileRunner);
        }

        public void Dispose()
        {
            foreach (var fileRunner in fileRunners)
            {
                fileRunner.Dispose();
            }
        }

        public IEnumerable<IScenarioRunner> FailedScenariosRunners()
        {
            return fileRunners
                .SelectMany(runner => runner.ScenarioRunners)
                .Where(scenario => scenario.Failed);
        }

        public int CountScenarios()
        {
            return FileRunners.Select(fileRunner => fileRunner.CountScenarioRunners())
                .Aggregate((value, total) => value + total);
        }

    }
}