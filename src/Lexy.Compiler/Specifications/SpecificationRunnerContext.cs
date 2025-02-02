using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language.Scenarios;
using Lexy.RunTime;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Specifications;

public class SpecificationRunnerContext : ISpecificationRunnerContext
{
    private readonly List<ISpecificationFileRunner> fileRunners = new();
    private readonly List<SpecificationsLogEntry> logEntries = new();
    private readonly ILogger<SpecificationsRunner> logger;
    private readonly DateTime startTimestamp;

    public int Failed { get; private set; }
    public IEnumerable<SpecificationsLogEntry> LogEntries => logEntries;
    public IReadOnlyCollection<ISpecificationFileRunner> FileRunners => fileRunners;

    public SpecificationRunnerContext(ILogger<SpecificationsRunner> logger)
    {
        startTimestamp = DateTime.Now;
        this.logger = logger;
    }

    public void Fail(Scenario scenario, string message, IEnumerable<string> errors, int? index = null)
    {
        Failed++;

        var suffix = index != null ? $"[{index}]" : "";
        var scenarioName = scenario.Name + suffix;
        var entry = new SpecificationsLogEntry(scenario.Reference, scenario, true,
            $"FAILED - {scenarioName}: {message}", errors);
        logEntries.Add(entry);

        logger.LogError("- FAILED  - {ScenarioName}: {Message}", scenarioName, message);
        errors?.ForEach(message => this.logger.LogInformation("  {Message}", message));
    }

    public void LogGlobal(string message)
    {
        var entry = new SpecificationsLogEntry(null, null, false, message);
        logEntries.Add(entry);
        logger.LogInformation("{Message}", message);
    }

    public void LogTimeSpent()
    {
        var difference = DateTime.Now.Subtract(this.startTimestamp).TotalMilliseconds;
        var message = $"Time: {difference} milliseconds";

        var entry = new SpecificationsLogEntry(null, null, false, message);
        logEntries.Add(entry);
        logger.LogInformation("Time: {Difference} milliseconds", difference);
    }

    public void Success(Scenario scenario, IEnumerable<ExecutionLogEntry> logging = null, int? index = null)
    {
        var suffix = index != null ? $"[{index}]" : "";
        var scenarioName = scenario.Name + suffix;

        var entry = new SpecificationsLogEntry(scenario.Reference, scenario, false, $"SUCCESS - {scenarioName}", null,
            logging);
        logEntries.Add(entry);
        logger.LogInformation("- SUCCESS - {ScenarioName}", scenarioName);
    }

    public void Add(ISpecificationFileRunner fileRunner)
    {
        fileRunners.Add(fileRunner);
    }

    public IEnumerable<IScenarioRunner> FailedScenariosRunners()
    {
        return fileRunners
            .SelectMany(runner => runner.ScenarioRunners)
            .Where(scenario => scenario.Failed);
    }

    public int CountScenarios()
    {
        return FileRunners.Sum(fileRunner => fileRunner.CountScenarioRunners());
    }
}