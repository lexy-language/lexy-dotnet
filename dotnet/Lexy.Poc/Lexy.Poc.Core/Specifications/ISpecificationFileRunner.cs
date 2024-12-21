using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.Core.Specifications
{
    public interface ISpecificationFileRunner : IDisposable
    {
        IEnumerable<IScenarioRunner> ScenarioRunners { get; }

        void Initialize(IServiceScope serviceScope, ISpecificationRunnerContext runnerContext, string fileName);

        int CountScenarioRunners();
        void Run();
    }
}