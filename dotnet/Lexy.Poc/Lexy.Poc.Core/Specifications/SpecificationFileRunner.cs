using System;
using System.Collections.Generic;
 using System.Linq;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationFileRunner : ISpecificationFileRunner
    {
        private ISpecificationRunnerContext runnerContext;
        private readonly IParserContext parserContext;
        private readonly ILexyParser parser;

        private string fileName;

        private IList<IScenarioRunner> scenarioRunners = new List<IScenarioRunner>();

        private IServiceScope serviceScope;

        public SpecificationFileRunner(ILexyParser parser, IParserContext parserContext)
        {
            this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
            this.parserContext = parserContext ?? throw new ArgumentNullException(nameof(parserContext));
        }

        public void Initialize(IServiceScope serviceScope, ISpecificationRunnerContext runnerContext, string fileName)
        {
            //runnerContext is managed by a parent ServiceProvider scope,
            //thus they can't be injected via the constructor

            if (this.fileName != null)
            {
                throw new InvalidOperationException("Each SpecificationFileRunner should only be initialized once. " +
                                                    "Use ServiceProvider.CreateScope to manage scope op each SpecificationFileRunner");
            }

            this.runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
            this.serviceScope = serviceScope;
            this.fileName = fileName;

            parser.ParseFile(fileName, false);

            scenarioRunners = parserContext
                .Components
                .GetScenarios()
                .Select(scenario => ScenarioRunner.Create(fileName, scenario, parserContext, runnerContext, this.serviceScope.ServiceProvider))
                .ToList();
        }

        public IEnumerable<IScenarioRunner> ScenarioRunners => scenarioRunners;

        public void Run()
        {
            runnerContext.LogGlobal($"{Environment.NewLine}Filename: {fileName}{Environment.NewLine}");

            foreach (var scenario in scenarioRunners)
            {
                scenario.Run();
            }
        }

        public static ISpecificationFileRunner Create(string fileName, IServiceProvider serviceProvider
            , ISpecificationRunnerContext runnerContext)
        {
            var serviceScope = serviceProvider.CreateScope();
            var runner = serviceScope.ServiceProvider.GetRequiredService<ISpecificationFileRunner>();
            runner.Initialize(serviceScope, runnerContext, fileName);
            return runner;
        }

        public void Dispose()
        {
            serviceScope?.Dispose();
            serviceScope = null;
        }

        public int CountScenarioRunners() => scenarioRunners.Count;
    }
}