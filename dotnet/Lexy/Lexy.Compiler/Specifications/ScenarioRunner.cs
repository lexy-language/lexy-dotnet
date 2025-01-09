using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Functions;
using Lexy.Compiler.Language.Scenarios;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Specifications;

public class ScenarioRunner : IScenarioRunner
{
    private readonly ILexyCompiler lexyCompiler;
    private readonly ISpecificationRunnerContext context;
    private readonly IParserLogger parserLogger;

    private readonly string fileName;
    private readonly Function function;
    private readonly RootNodeList rootNodeList;

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
        function = scenario.Function ?? rootNodeList.GetFunction(scenario.FunctionName.Value);
    }

    public void Run()
    {
        if (parserLogger.NodeHasErrors(Scenario))
        {
            Fail($"  Parsing scenario failed: {Scenario.FunctionName}");
            parserLogger.ErrorNodeMessages(Scenario).ForEach(context.Log);
            return;
        }

        if (!ValidateErrors(context)) return;

        var nodes = function.GetFunctionAndDependencies(rootNodeList);

        using var compilerResult = lexyCompiler.Compile(nodes);

        var executable = compilerResult.GetFunction(function);
        var values = GetValues(Scenario.Parameters, function.Parameters, compilerResult);

        var executionContext = compilerResult.CreateContext();
        var result = executable.Run(executionContext, values);

        var validationResultText = GetValidationResult(result, compilerResult);
        if (validationResultText.Length > 0)
        {
            Fail(validationResultText);
        }
        else
        {
            context.Success(Scenario);
        }
    }

    public string ParserLogging()
    {
        return $"------- Filename: {fileName}{Environment.NewLine}{parserLogger.ErrorMessages().Format(2)}";
    }

    private void Fail(string message)
    {
        Failed = true;
        context.Fail(Scenario, message);
    }

    private string GetValidationResult(FunctionResult result, CompilationResult compilationResult)
    {
        var validationResult = new StringWriter();
        foreach (var expected in Scenario.Results.Assignments)
        {
            var actual = result.GetValue(expected.Variable);
            var expectedValue =
                TypeConverter.Convert(compilationResult, expected.ConstantValue.Value, expected.VariableType);

            if (actual == null || expectedValue == null
                               || actual.GetType() != expectedValue.GetType()
                               || Comparer.Default.Compare(actual, expectedValue) != 0)
            {
                validationResult.WriteLine(
                    $"'{expected.Variable}' should be '{expectedValue ?? "<null>"}' ({expectedValue?.GetType().Name}) but is '{actual ?? "<null>"} ({actual?.GetType().Name})'");
            }
        }

        return validationResult.ToString();
    }

    private bool ValidateErrors(ISpecificationRunnerContext runnerContext)
    {
        if (Scenario.ExpectRootErrors.HasValues) return ValidateRootErrors();

        var node = function ?? Scenario.Function ?? Scenario.Enum ?? (IRootNode)Scenario.Table;
        var failedMessages = parserLogger.ErrorNodeMessages(node);

        if (failedMessages.Length > 0 && !Scenario.ExpectError.HasValue)
        {
            Fail("Exception occurred: " + failedMessages.Format(2));
            return false;
        }

        if (!Scenario.ExpectError.HasValue) return true;

        if (failedMessages.Length == 0)
        {
            Fail($"No exception {Environment.NewLine}" +
                 $"  Expected: {Scenario.ExpectError.Message}{Environment.NewLine}");
            return false;
        }

        if (!failedMessages.Any(message => message.Contains(Scenario.ExpectError.Message)))
        {
            Fail($"Wrong exception {Environment.NewLine}" +
                 $"  Expected: {Scenario.ExpectError.Message}{Environment.NewLine}" +
                 $"  Actual: {failedMessages.Format(4)}");
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
            Fail($"No exceptions {Environment.NewLine}" +
                 $"  Expected: {Scenario.ExpectRootErrors.Messages.Format(4)}{Environment.NewLine}" +
                 "  Actual: none");
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

        Fail($"Wrong exception {Environment.NewLine}" +
             $"  Expected: {Scenario.ExpectRootErrors.Messages.Format(4)}{Environment.NewLine}" +
             $"  Actual: {parserLogger.ErrorMessages().Format(4)}");
        return false;
    }

    private IDictionary<string, object> GetValues(ScenarioParameters scenarioParameters,
        FunctionParameters functionParameters, CompilationResult compilationResult)
    {
        var result = new Dictionary<string, object>();
        foreach (var parameter in scenarioParameters.Assignments)
        {
            var type = functionParameters.Variables.FirstOrDefault(variable =>
                variable.Name == parameter.Variable.ParentIdentifier);
            if (type == null)
            {
                throw new InvalidOperationException(
                    $"Function '{function.NodeName}' parameter '{parameter.Variable.ParentIdentifier}' not found.");
            }

            var value = GetValue(compilationResult, parameter.ConstantValue.Value, parameter.VariableType);
            result.Add(parameter.Variable.ToString(), value);
        }

        return result;
    }

    private object GetValue(CompilationResult compilationResult, object value, VariableType type)
    {
        return TypeConverter.Convert(compilationResult, value, type);
    }
}