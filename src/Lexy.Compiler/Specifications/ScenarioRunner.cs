using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.DependencyGraph;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Specifications;

public class ScenarioRunner : IScenarioRunner
{
    private readonly string fileName;

    private readonly ILexyCompiler lexyCompiler;
    private readonly ISpecificationRunnerContext context;
    private readonly IParserLogger parserLogger;
    private readonly IComponentNodeList componentNodes;

    private Function function;

    public bool Failed { get; private set; }
    public Scenario Scenario { get; }

    public ScenarioRunner(string fileName, ILexyCompiler lexyCompiler, IComponentNodeList componentNodes, Scenario scenario,
        ISpecificationRunnerContext context, IParserLogger parserLogger)
    {
        this.lexyCompiler = lexyCompiler;
        this.fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        this.context = context;

        this.componentNodes = componentNodes ?? throw new ArgumentNullException(nameof(componentNodes));
        this.parserLogger = parserLogger ?? throw new ArgumentNullException(nameof(parserLogger));

        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
    }

    public int CountScenarios()
    {
        return Scenario.ValidationTable != null ? Scenario.ValidationTable.Rows.Count : 1;
    }

    public void Run()
    {
        function = GetFunctionNode(componentNodes, Scenario);

        if (!ValidateScenarioErrors()) return;
        if (!ValidateErrors()) return;

        var nodes = DependencyGraphFactory.NodeAndDependencies(componentNodes, function);

        using var compilerResult = lexyCompiler.Compile(nodes);

        var executable = compilerResult.GetFunction(function);
        if (Scenario.ValidationTable != null)
        {
            RunFunctionWithValidationTable(executable, compilerResult, function);
        }
        else
        {
            var values = Scenario.Parameters.GetValues();
            RunFunctionWithValues(values, null, compilerResult, executable);
        }
    }

    private void RunFunctionWithValidationTable(ExecutableFunction executable,
        ICompilationResult compilationResult,
        Function functionNode)
    {
        if (Scenario.ValidationTable == null || Scenario.ValidationTable.Header == null) return;
        foreach (var row in Scenario.ValidationTable.Rows)
        {
            var values = row.GetValues(functionNode, Scenario.ValidationTable.Header);
            RunFunctionWithValues(values, row, compilationResult, executable);
        }
    }

    private void RunFunctionWithValues(IDictionary<string, object> values, ValidationTableRow tableRow,
        ICompilationResult compilationResult, ExecutableFunction executable)
    {
        var result = RunFunction(executable, values);
        if (result == null) return;

        if (!ValidateExecutionLogging(result)) return;

        var validationResultText = ValidateResult(result, compilationResult, tableRow);
        if (validationResultText.Count > 0)
        {
            Fail("Results validation failed.", validationResultText, tableRow?.Index);
        }
        else
        {
            context.Success(Scenario, result.Logging, tableRow?.Index);
        }
    }

    private Function GetFunctionNode(IComponentNodeList componentNodes, Scenario scenario)
    {
        if (scenario.Function != null)
        {
            return scenario.Function;
        }

        if (scenario.FunctionName != null)
        {
            var functionNode = componentNodes.GetFunction(scenario.FunctionName.Value);
            if (functionNode == null)
            {
                Fail($"Unknown function: " + scenario.FunctionName, parserLogger.ErrorNodeMessages(Scenario));
            }

            return functionNode;
        }

        return null;
    }

    private FunctionResult RunFunction(ExecutableFunction executable, IDictionary<string, object> values)
    {
        try
        {
            return executable.Run(values);
        }
        catch (Exception exception)
        {
            if (!ValidateExecutionErrors(exception))
            {
                Fail("Execution error occured.", new[]
                {
                    "Error: ", exception.ToString()
                });
            }

            return null;
        }
    }

    public string ParserLogging()
    {
        return $"------- Filename: {fileName}{Environment.NewLine}{parserLogger.ErrorMessages().Format(2)}";
    }

    private void Fail(string message, IEnumerable<string> errors, int? index = null)
    {
        Failed = true;
        context.Fail(Scenario, message, errors, index);
    }

    private IReadOnlyList<string> ValidateResult(FunctionResult result, ICompilationResult compilationResult,
        ValidationTableRow tableRow)
    {
        var validationResult = new List<string>();
        if (tableRow != null)
        {
            ValidateTableResults(tableRow, result, validationResult);
        }

        if (Scenario.Results != null)
        {
            ValidateResults(result, compilationResult, validationResult);
        }

        return validationResult;
    }

    private void ValidateResults(FunctionResult result, ICompilationResult compilationResult,
        List<string> validationResult)
    {
        foreach (var expected in Scenario.Results.AllAssignments())
        {
            var actual = result.GetValue(expected.Variable);
            var expectedValue =
                TypeConverter.Convert(compilationResult, expected.ConstantValue.Value, expected.VariableType);

            if (actual == null
             || expectedValue == null
             || actual.GetType() != expectedValue.GetType()
             || Comparer.Default.Compare(actual, expectedValue) != 0)
            {
                validationResult.Add(
                    $"'{expected.Variable}' should be '{expectedValue ?? "<null>"}' ({expectedValue?.GetType().Name}) but is '{actual ?? "<null>"} ({actual?.GetType().Name})'");
            }
        }
    }

    private void ValidateTableResults(ValidationTableRow tableRow,
        FunctionResult result,
        IList<string> validationResult)
    {
        for (var index = 0; index < tableRow.Values.Count; index++)
        {
            var column = Scenario.ValidationTable?.Header?.GetColumn(index);
            if (column == null) continue;
            var variable = IdentifierPath.Parse(column.Name);
            if (!IsResult(variable)) continue;
            var expected = tableRow.Values[index];
            ValidateRowValueResult(variable, expected, result, validationResult);
        }
    }

    private bool IsResult(IdentifierPath path)
    {
        if (function.Results == null) return false;
        return function.Results.Variables.Any(result => result.Name == path.RootIdentifier);
    }

    private static void ValidateRowValueResult(IdentifierPath path, ValidationTableValue value,
        FunctionResult result,
        IList<string> validationResult)
    {
        var actual = result.GetValue(path);
        var expectedValue = value.GetValue();
        if (actual == null || expectedValue == null || !actual.Equals(expectedValue))
        {
            validationResult.Add(
                $"'{path}' should be '{expectedValue ?? "<null>"}' ({expectedValue?.GetType().Name}) but is '{actual ?? "<null>"}' ({actual?.GetType().Name})");
        }
    }

    private IReadOnlyList<string> GetDependenciesErrors()
    {
        var node = function
                ?? Scenario.Function
                ?? Scenario.Enum
                ?? (IComponentNode)Scenario.Table;
        if (node == null)
        {
            Fail("Scenario has no function, enum or table.", Array.Empty<string>());
            return null;
        }

        var dependencies = DependencyGraphFactory.NodeAndDependencies(componentNodes, node);
        return parserLogger.ErrorNodesMessages(dependencies);
    }

    private bool ValidateScenarioErrors()
    {
        var failedMessages = parserLogger.ErrorNodeMessages(Scenario);
        if (failedMessages.Length == 0) return true;

        var skipValidationWhenExecutionErrorsAreExpected = failedMessages.Length > 0
                                                           && Scenario.ExpectExecutionErrors != null;

        if (skipValidationWhenExecutionErrorsAreExpected) return true;

        var failedDependenciesMessages = GetDependenciesErrors();

        var allErrors = failedDependenciesMessages != null
            ? failedMessages.Union(failedDependenciesMessages).ToList()
            : failedMessages.ToList();
        var expectErrors = Scenario.ExpectErrors;
        return ValidateExpectedErrors("Parsing Scenario", allErrors, expectErrors?.Messages);
    }

    private bool ValidateErrors()
    {
        if (Scenario.ExpectComponentErrors?.HasValues == true) return ValidateComponentErrors();

        var failedMessages = GetDependenciesErrors();
        if (failedMessages == null) return false;

        var expectErrors = Scenario.ExpectErrors;
        return ValidateExpectedErrors("Parsing", failedMessages, expectErrors?.Messages);
    }

    private bool ValidateComponentErrors()
    {
        var failedMessages = parserLogger.ErrorMessages();
        return ValidateExpectedErrors("Component", failedMessages, Scenario.ExpectComponentErrors?.Messages);
    }

    private bool ValidateExecutionErrors(Exception exception)
    {
        var errors = ExceptionErrorsWithoutEmptyLinesAndStackTrace(exception);
        return !ValidateExpectedErrors("Execution", errors,
            Scenario.ExpectExecutionErrors?.Messages);
    }

    private static IReadOnlyList<string> ExceptionErrorsWithoutEmptyLinesAndStackTrace(Exception exception)
    {
        return exception.ToString().Split("\n")
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("at "))
            .ToList();
    }

    private bool ValidateExpectedErrors(string title,
        IReadOnlyList<string> actualErrors,
        IEnumerable<string> expectErrors)
    {
        if (actualErrors.Count > 0 && expectErrors == null)
        {
            Fail($"{title} error(s) occurred", StringArrayBuilder
                .New("Expected:").List(actualErrors).Array());
            return false;
        }

        if (expectErrors == null || !expectErrors.Any()) return true;

        if (actualErrors.Count == 0)
        {
            Fail($"No {title} errors, but errors expected:", StringArrayBuilder
                .New("Expected:").List(expectErrors).Array());
            return false;
        }

        var notFound = new List<string>();
        var remainingMessages = actualErrors.ToList();
        foreach (var expectError in expectErrors)
        {
            var failedMessage = remainingMessages.FirstOrDefault(message => message.Contains(expectError));
            if (failedMessage != null)
            {
                remainingMessages.Remove(failedMessage);
            }
            else
            {
                notFound.Add(expectError);
            }
        }

        if (remainingMessages.Any() || notFound.Any())
        {
            Fail($"Wrong {title} error(s) occurred.", StringArrayBuilder
                .New("Expected:").List(MarkAsFound(expectErrors, notFound))
                .Add("Actual:").List(MarkAsFound(actualErrors, remainingMessages))
                .Array());
            return false;
        }

        context.Success(Scenario);
        return false;
    }

    private IReadOnlyList<string> MarkAsFound(IEnumerable<string> expectErrors,
        IReadOnlyList<string> remainingMessages)
    {
        return expectErrors
            .Select(expectError => remainingMessages.Contains(expectError)
                ? $"(not found) {expectError}"
                : $"(found)     {expectError}")
            .ToList();
    }

    private bool ValidateExecutionLogging(FunctionResult result)
    {
        if (Scenario.ExecutionLogging == null) return true;
        var errors = Scenario.ExecutionLogging.Entries.ValidateExecutionLogging(result.Logging);
        if (errors != null)
        {
            foreach (var error in errors)
            {
                parserLogger.Fail(Scenario.Reference, error);
            }
            Fail("Invalid Execution Logging", errors);
            return false;
        }

        return true;
    }
}