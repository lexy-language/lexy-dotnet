using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Language;
using Microsoft.Extensions.Logging;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationRunnerContext : ISpecificationRunnerContext, IDisposable
    {
        private readonly List<ISpecificationFileRunner> fileRunners = new List<ISpecificationFileRunner>();

        private readonly ILogger<SpecificationRunnerContext> logger;

        //public IList<string> Messages { get; } = new List<string>();

        public int Failed { get; private set; }

        public IReadOnlyCollection<ISpecificationFileRunner> FileRunners => fileRunners;

        public SpecificationRunnerContext(ILogger<SpecificationRunnerContext> logger)
        {
            this.logger = logger;
        }

        public void Fail(Scenario scenario, string message)
        {
            Failed++;

            var log = $"- FAILED  - {scenario.Name}: {message}";

            Console.WriteLine(log);
            logger.LogError(log);
        }

        public void LogGlobal(string message)
        {
            Console.WriteLine(  Environment.NewLine + message + Environment.NewLine);
            logger.LogInformation(message);
        }

        public void Log(string message)
        {
            var log = $"  {message}";
            Console.WriteLine(log);
            logger.LogInformation(log);
        }

        public void Success(Scenario scenario)
        {
            var log = $"- SUCCESS - {scenario.Name}";
            Console.WriteLine(log);
            logger.LogInformation(log);
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