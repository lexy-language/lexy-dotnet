using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Specifications;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Lexy.Tests;

public static class RunScenariosExtensions
{
    public static IEnumerable<SpecificationsLogEntry> RunScenarios(this IServiceProvider serviceProvider,  string currentFileName, ComponentNodeList nodes, IParserLogger parserLogger, Dependencies dependencies)
    {
        var compiler = serviceProvider.GetRequiredService<ILexyCompiler>();
        var logger = new NullLogger<SpecificationsRunner>();
        var context = new SpecificationRunnerContext(logger);

        RunRunners(currentFileName, nodes, parserLogger, dependencies, compiler, context);

        context.LogTimeSpent();

        return context.LogEntries;
    }

    private static void RunRunners(string currentFileName, ComponentNodeList nodes, IParserLogger parserLogger, Dependencies dependencies, ILexyCompiler lexyCompiler, SpecificationRunnerContext context)
    {
        foreach (var scenario in nodes.OfType<Scenario>())
        {
            var runner = new ScenarioRunner(currentFileName, lexyCompiler, nodes, scenario, context, parserLogger, dependencies);
            GlobalTiming.Log(currentFileName + "-" + scenario.Name);
            runner.Run();
            GlobalTiming.Log(currentFileName + "-" + scenario.Name + ":runned");
        }
    }
}
