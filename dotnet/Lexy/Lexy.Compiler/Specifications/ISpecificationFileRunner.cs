using System.Collections.Generic;

namespace Lexy.Compiler.Specifications;

public interface ISpecificationFileRunner
{
    IEnumerable<IScenarioRunner> ScenarioRunners { get; }

    int CountScenarioRunners();
    void Run();
}