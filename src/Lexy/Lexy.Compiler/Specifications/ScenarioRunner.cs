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
        function = GetScenarioFunction(rootNodeList, scenario);
    }

    private static Function GetScenarioFunction(RootNodeList rootNodeList, Scenario scenario)
    {
        return scenario.Function
            ?? (scenario.FunctionName != null ? rootNodeList.GetFunction(scenario.FunctionName.Value) : null);
    }

    public void Run()
    {
        if (parserLogger.NodeHasErrors(Scenario) && Scenario.ExpectExecutionErrors?.HasValues != true)
        {
            Fail($"  Parsing scenario failed: {Scenario.FunctionName}");
            parserLogger.ErrorNodeMessages(Scenario).ForEach(context.Log);
            return;
        }

        if (!ValidateErrors(context)) return;

        var nodes = function.GetFunctionAndDependencies(rootNodeList);

        using var compilerResult = lexyCompiler.Compile(nodes);

        var executable = compilerResult.GetFunction(function);
        var values = GetValues(Scenario.Parameters);

        var result = RunFunction(executable, values);
        if (result == null) return;

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
                Fail("No execution error expected. Execution raised: " + exception);
            }

            return null;
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

    private string GetValidationResult(FunctionResult result, ICompilationResult compilationResult)
    {
        if (Scenario.Results == null) return string.Empty;

        var validationResult = new StringWriter();
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
                validationResult.WriteLine(
                    $"'{expected.Variable}' should be '{expectedValue ?? "<null>"}' ({expectedValue?.GetType().Name}) but is '{actual ?? "<null>"} ({actual?.GetType().Name})'");
            }
        }

        return validationResult.ToString();
    }

    private bool ValidateErrors(ISpecificationRunnerContext runnerContext)
    {
        if (Scenario.ExpectRootErrors?.HasValues == true) return ValidateRootErrors();

        var node = function ?? Scenario.Function ?? Scenario.Enum ?? (IRootNode)Scenario.Table;
        var failedMessages = parserLogger.ErrorNodeMessages(node);

        if (failedMessages.Length > 0 && !Scenario.ExpectError.HasValue)
        {
            Fail("Exception occurred: " + failedMessages.Format(2));
            return false;
        }

        if (Scenario.ExpectError?.HasValue != true) return true;

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
            Fail($"Execution error not found\n Not found: {expected.Format(2)}\n  Actual: {errorMessage}");
        }

        return true;
    }

    private IDictionary<string, object> GetValues(ScenarioParameters scenarioParameters)
    {
        var result = new Dictionary<string, object>();
        if (scenarioParameters == null) return result;

        foreach (var parameter in scenarioParameters.AllAssignments())
        {
            SetParametersValue(parameter, result);
        }

        return result;
    }

    private static void SetParametersValue(AssignmentDefinition parameter,
        Dictionary<string, object> result)
    {
        var reference = parameter.Variable;
        var valueObject = result;
        while (reference.Parts > 1)
        {
            if (!valueObject.ContainsKey(reference.ParentIdentifier))
            {
                var childObject = new Dictionary<string, object>();
                valueObject.Add(reference.ParentIdentifier, childObject);
            }

            if (valueObject[reference.ParentIdentifier] is not Dictionary<string, object> dictionary)
            {
                throw new InvalidOperationException(
                    $"Parent variable '{reference.ParentIdentifier}' of parameter '{parameter.Variable}' already set to value: {valueObject[reference.ParentIdentifier].GetType()}");
            }

            valueObject = dictionary;
            reference = reference.ChildrenReference();
        }

        var value = parameter.ConstantValue.Value;
        valueObject.Add(reference.ParentIdentifier, value);
    }
}