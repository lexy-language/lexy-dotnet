using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Specifications;

public class SpecificationFileRunner : ISpecificationFileRunner
{
    private readonly ILexyCompiler compiler;
    private readonly ILexyParser parser;

    private readonly string fileName;
    private readonly ISpecificationRunnerContext runnerContext;

    private readonly List<IScenarioRunner> scenarioRunners = new();
    private ParserResult result;

    public IEnumerable<IScenarioRunner> ScenarioRunners => scenarioRunners;

    public SpecificationFileRunner(string fileName, ILexyParser parser, ISpecificationRunnerContext runnerContext, ILexyCompiler compiler)
    {
        this.fileName = fileName;
        this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        this.runnerContext = runnerContext ?? throw new ArgumentNullException(nameof(runnerContext));
        this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
    }

    public void Initialize()
    {
        result = parser.ParseFile(fileName, false);

        var runners = result
            .Nodes
            .GetScenarios()
            .Select(scenario => CreateScenarioRunner(scenario, runnerContext, result.Nodes, result.Logger))
            .ToList();

        scenarioRunners.AddRange(runners);
    }

    public void Run()
    {
        ValidateHasScenarioCheckingRootErrors(result.Logger);

        if (scenarioRunners.Count == 0) return;

        runnerContext.LogGlobal($"Filename: {fileName}");

        foreach (var scenario in scenarioRunners) scenario.Run();
    }

    private ScenarioRunner CreateScenarioRunner(Scenario scenario, ISpecificationRunnerContext context,
        RootNodeList nodes, IParserLogger logger)
    {   
        return new ScenarioRunner(fileName, compiler, nodes, scenario, context, logger);
    }

    public int CountScenarioRunners()
    {
        return scenarioRunners.Count;
    }

    private void ValidateHasScenarioCheckingRootErrors(IParserLogger logger)
    {
        if (!logger.HasRootErrors()) return;

        var rootScenarioRunner =
            scenarioRunners.FirstOrDefault(runner => runner.Scenario.ExpectRootErrors.HasValues);

        if (rootScenarioRunner == null)
            throw new InvalidOperationException(
                $"{fileName} has root errors but no scenario that verifies expected root errors. Errors: {logger.ErrorRootMessages().Format(2)}");
    }
}