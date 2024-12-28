using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Compiler.Specifications;

public interface IScenarioRunner
{
    bool Failed { get; }
    Scenario Scenario { get; }

    void Initialize(string fileName, RootNodeList rootNodeList, Scenario scenario,
        ISpecificationRunnerContext context, IServiceScope serviceScope, IParserLogger parserLogger);

    void Run();
    string ParserLogging();
}