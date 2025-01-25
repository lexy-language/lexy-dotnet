using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Compiler.Specifications;

public interface IScenarioRunner
{
    bool Failed { get; }
    Scenario Scenario { get; }

    void Run();
    string ParserLogging();
}