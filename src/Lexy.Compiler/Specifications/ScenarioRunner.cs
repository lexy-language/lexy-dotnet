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
    private readonly ILexyCompiler lexyCompiler;
    private readonly ISpecificationRunnerContext context;
    private readonly IParserLogger parserLogger;

    private readonly string fileName;
    private readonly RootNodeList rootNodeList;

    private Function function;

    public bool Failed { get; private set; }
    public Scenario Scenario { get; }

    public ScenarioRunner(string fileName, ILexyCompiler lexyCompiler, RootNodeList rootNodeList, Scenario scenario,
        ISpecificationRunnerContext context, IParserLogger parserLogger)
    {
        this.lexyCompiler = lexyCompiler;
        this.fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        this.context = context;

        this.rootNodeList = rootNodeList ?? throw new ArgumentNullException(nameof(rootNodeList));
        this.parserLogger = parserLogger ?? throw new ArgumentNullException(nameof(parserLogger));

        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
    }

    public int CountScenarios()
    {
        return Scenario.ValidationTable != null ? Scenario.ValidationTable.Rows.Count : 1;
    }

    public void Run()
    {
        function = GetFunctionNode(rootNodeList, Scenario);
        if (parserLogger.NodeHasErrors(Scenario) && Scenario.ExpectExecutionErrors?.HasValues != true)
        {
            Fail($"  Parsing scenario failed: {Scenario.FunctionName}",
                parserLogger.ErrorNodeMessages(Scenario));
            return;
        }

        if (!ValidateErrors(context)) return;

        var nodes = DependencyGraphFactory.NodeAndDependencies(rootNodeList, function);

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
        Function functionNode) {
        if (Scenario.ValidationTable == null || Scenario.ValidationTable.Header == null) return;
        foreach (var row in Scenario.ValidationTable.Rows) {
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

    private Function GetFunctionNode(RootNodeList rootNodeList, Scenario scenario)
    {
        if (scenario.Function != null)
        {
            return scenario.Function;
        }

        if (scenario.FunctionName != null)
        {
            var functionNode = rootNodeList.GetFunction(scenario.FunctionName.Value);
            if (functionNode == null) {
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
                Fail("Execution error occured.", new [] {
                    "Error: ",exception.ToString()});
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

    private IReadOnlyList<string> ValidateResult(FunctionResult result, ICompilationResult compilationResult, ValidationTableRow tableRow)
    {
        var validationResult = new List<string>();
        if (tableRow != null) {
            ValidateTableResults(tableRow, result, validationResult);
        }
        if (Scenario.Results != null)
        {
            ValidateResults(result, compilationResult, validationResult);
        }
        return validationResult;
    }

    private void ValidateResults(FunctionResult result, ICompilationResult compilationResult, List<string> validationResult)
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
        List<string> validationResult) {

        for (var index = 0; index < tableRow.Values.Count; index++) {
            var column = Scenario.ValidationTable?.Header?.GetColumn(index);
            if (column == null) continue;
            var variable = VariablePathParser.Parse(column.Name);
            if (!IsResult(variable)) continue;
            var expected = tableRow.Values[index];
            ValidateRowValueResult(variable, expected, result, validationResult);
        }
    }

    private bool IsResult(VariablePath path)
    {
        if (function.Results == null) return false;
        return function.Results.Variables.Any(result => result.Name == path.ParentIdentifier);
    }

    private static void ValidateRowValueResult(VariablePath path, ValidationTableValue value,
        FunctionResult result,
        IList<string> validationResult) {
        var actual = result.GetValue(path);
        var expectedValue = value.GetValue();
        if (actual == null || expectedValue == null || !actual.Equals(expectedValue)) {
            validationResult.Add(
                $"'{path}' should be '{expectedValue ?? "<null>"}' ({expectedValue?.GetType().Name}) but is '{actual ?? "<null>"}' ({actual?.GetType().Name})");
        }
    }

    private bool ValidateErrors(ISpecificationRunnerContext runnerContext)
    {
        if (Scenario.ExpectRootErrors?.HasValues == true) return ValidateRootErrors();

        var node = function
               ?? Scenario.Function
               ?? Scenario.Enum
               ?? (IRootNode)Scenario.Table;
        if (node == null) {
            Fail("Scenario has no function, enum or table.", Array.Empty<string>());
            return false;
        }

        var dependencies = DependencyGraphFactory.NodeAndDependencies(rootNodeList, node);
        var failedMessages = parserLogger.ErrorNodesMessages(dependencies);

        if (failedMessages.Length > 0 && Scenario.ExpectErrors?.HasValues != true)
        {
            Fail("Exception occurred: ", failedMessages);
            return false;
        }

        if (Scenario.ExpectErrors?.HasValues != true) return true;

        if (failedMessages.Length == 0)
        {
            Fail($"No errors but errors expected:", Scenario.ExpectErrors.Messages);
            return false;
        }

        var errorNotFound = Scenario.ExpectErrors.Messages.Any(message =>
            !failedMessages.Any(failedMessage => failedMessage.Contains(message)));

        if (errorNotFound)
        {
            Fail($"Wrong error(s) occurred", StringArrayBuilder
                .New("Expected:").List(Scenario.ExpectErrors.Messages)
                .Add("Actual:").List(failedMessages).Array());
            return false;
        }

        runnerContext.Success(Scenario);
        return false;
    }

    private bool ValidateRootErrors()
    {
        var failedMessages = parserLogger.ErrorMessages().ToList();
        if (!failedMessages.Any())
        {
            Fail($"Root errors expected. No errors occurred", StringArrayBuilder
                .New("Expected:").List(Scenario.ExpectRootErrors.Messages)
                .Array());
            return false;
        }

        var failed = false;
        foreach (var rootMessage in Scenario.ExpectRootErrors.Messages)
        {
            var failedMessage = failedMessages.Find(message => message.Contains(rootMessage));
            if (failedMessage != null)
                failedMessages.Remove(failedMessage);
            else
                failed = true;
        }

        if (!failedMessages.Any() && !failed)
        {
            context.Success(Scenario);
            return false; // don't compile and run rest of scenario
        }

        Fail($"Wrong root error(s) occurred.", StringArrayBuilder
            .New("Expected:").List(Scenario.ExpectRootErrors.Messages)
            .Add("Actual: ").List(parserLogger.ErrorMessages())
            .Array());
        return false;
    }


    private bool ValidateExecutionErrors(Exception exception)
    {
        if (Scenario.ExpectExecutionErrors?.HasValues != true) return false;

        var errorMessage = exception.ToString();
        var failedErrors = new List<string>();
        var expected = Scenario.ExpectExecutionErrors.Messages.ToList();

        foreach (var error in Scenario.ExpectExecutionErrors.Messages)
        {
            if (!errorMessage.Contains(error))
            {
                failedErrors.Add(error);
            }
            else
            {
                expected.Remove(error);
            }
        }

        if (failedErrors.Count > 0)
        {
            Fail($"Execution error not found", StringArrayBuilder
                .New("Not found:").List(expected)
                .Add("Actual:").Add(errorMessage, 2)
                .Array());
        }

        return true;
    }

    private bool ValidateExecutionLogging(FunctionResult result) {
        if (Scenario.ExecutionLogging == null) return true;
        var errors = Scenario.ExecutionLogging.Entries.ValidateExecutionLogging(result.Logging);
        if (errors != null) {
            Fail("Invalid Execution Logging", errors);
            return false;
        }
        return true;
    }
}
