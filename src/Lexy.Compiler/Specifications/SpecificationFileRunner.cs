using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;
using Lexy.RunTime;

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
        this.parser = Assert.NotNull(parser, nameof(parser));
        this.runnerContext = Assert.NotNull(runnerContext, nameof(runnerContext));
        this.compiler = Assert.NotNull(compiler, nameof(compiler));
    }

    public async Task Initialize()
    {
        result = await Parse();

        result
            .Nodes
            .GetScenarios()
            .ForEach(scenario =>
            {
                var scenarioRunner = CreateScenarioRunner(scenario, runnerContext, result.Nodes, result.Logger, result.Dependencies);
                scenarioRunners.Add(scenarioRunner);
            });
    }

    private async Task<ParserResult> Parse()
    {
        try
        {
            return await parser.ParseFile(fileName, new ParseOptions { SuppressException = true });
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Error while parsing " + fileName, exception);
        }
    }

    public void Run()
    {
        ValidateHasScenarioCheckingComponentErrors(result.RootNode.Reference, result.Logger);

        if (scenarioRunners.Count == 0) return;

        runnerContext.LogGlobal($"Filename: {fileName}");

        foreach (var scenario in scenarioRunners)
        {
            Run(scenario);
        }
    }

    private void Run(IScenarioRunner scenario)
    {
        try
        {
            scenario.Run();
        }
        catch (Exception innerException)
        {
            throw new InvalidOperationException("Error occured while running: " + fileName, innerException);
        }
    }

    private ScenarioRunner CreateScenarioRunner(Scenario scenario, ISpecificationRunnerContext context,
        ComponentNodeList nodes, IParserLogger logger, Dependencies dependencies)
    {
        try
        {
            return new ScenarioRunner(fileName, compiler, nodes, scenario, context, logger, dependencies);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException("Error occurred while create runner for: " + fileName, exception);
        }
    }

    public int CountScenarioRunners()
    {
        return scenarioRunners.Sum(runner => runner.CountScenarios());
    }

    private void ValidateHasScenarioCheckingComponentErrors(SourceReference reference, IParserLogger logger)
    {
        if (!logger.HasComponentErrors()) return;

        var componentScenarioRunner = scenarioRunners.FirstOrDefault(runner => runner.Scenario.ExpectComponentErrors?.HasValues == true);

        if (componentScenarioRunner == null)
        {
            logger.Fail(reference,
                $"'{fileName}' has component errors but no scenario that verifies expected root errors. Errors: {logger.ErrorComponentMessages().Format(2)}");
        }
    }
}
