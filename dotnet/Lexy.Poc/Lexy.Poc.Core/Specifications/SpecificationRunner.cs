using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationRunner
    {
        private readonly Scenario scenario;
        private readonly Function function;
        private readonly SpecificationRunnerContext context;
        private readonly ParserContext parserContext;
        private readonly Components components;

        public string FileName { get; }
        public bool Failed { get; private set; }

        private SpecificationRunner(string fileName, ParserContext parserContext, Components components,
            Scenario scenario, Function function, SpecificationRunnerContext context)
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

            this.parserContext = parserContext;
            this.components = components;
            this.scenario = scenario;
            this.function = function;
            this.context = context;
        }

        public static SpecificationRunner Create(string fileName, Scenario scenario,
            SpecificationRunnerContext runnerContext, ParserContext parserContext)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (parserContext == null) throw new ArgumentNullException(nameof(context));

            var components = parserContext.Components;
            var function = components.GetFunction(scenario.FunctionName.Value);

            return new SpecificationRunner(fileName, parserContext, components, scenario, function, runnerContext);
        }


        public void Run()
        {
            if (scenario.HasErrors)
            {
                Fail($"  Parsing scenario failed: {scenario.FunctionName}");
                scenario.FailedMessages.ForEach(context.Log);
                return;
            }

            if (function == null)
            {
                Fail($"  Function not found: {scenario.FunctionName}");
                return;
            }

            if (ValidateErrors(context)) return;

            var compiler = new LexyCompiler();
            var environment = compiler.Compile(components, function);
            var executable = environment.GetFunction(function);
            var values = GetValues(scenario.Parameters, function.Parameters, environment);

            var result = executable.Run(values);

            var validationResultText = GetValidationResult(result, environment);
            if (validationResultText.Length > 0)
            {
                Fail(validationResultText);

            }
            else
            {
                context.Success(scenario);
            }
        }

        private void Fail(string message)
        {
            Failed = true;
            context.Fail(scenario, message);
        }

        private string GetValidationResult(FunctionResult result, ExecutionEnvironment environment)
        {
            var validationResult = new StringWriter();
            foreach (var expected in scenario.Results.Assignments)
            {
                var actual = result[expected.Name];

                var expectedValue = TypeConverter.Convert(environment, expected.Value,
                    function.Results.GetParameterType(expected.Name));

                if (Comparer.Default.Compare(actual, expectedValue) != 0)
                {
                    validationResult.WriteLine($"'{expected.Name}' should be '{expectedValue}' but is '{actual}'");
                }
            }

            return validationResult.ToString();
        }

        private bool ValidateErrors(SpecificationRunnerContext context)
        {
            if (function.FailedMessages.Any())
            {
                ValidateFunctionErrors(context);
                return true;
            }

            if (scenario.ExpectError.HasValue)
            {
                Fail($"Exception expected but didn't occur: {scenario.ExpectError.Message}");
                return true;
            }

            return false;
        }

        private void ValidateFunctionErrors(SpecificationRunnerContext context)
        {
            if (!scenario.ExpectError.HasValue)
            {
                Fail("Exception occured: " + Format(function.FailedMessages));
                return;
            }

            foreach (var message in function.FailedMessages)
            {
                if (!message.Contains(scenario.ExpectError.Message))
                {
                    Fail($"Wrong exception {Environment.NewLine}  Expected: {scenario.ExpectError.Message}{Environment.NewLine}  Actual: {message}");
                    return;
                }
            }

            context.Success(scenario);
        }

        private string Format(IEnumerable<string> functionFailedMessages)
        {
            return string.Join(Environment.NewLine, functionFailedMessages);
        }

        private IDictionary<string, object> GetValues(ScenarioParameters scenarioParameters,
            FunctionParameters functionParameters, ExecutionEnvironment environment)
        {
            var result = new Dictionary<string, object>();
            foreach (var parameter in scenarioParameters.Assignments)
            {
                var type = functionParameters.Variables.FirstOrDefault(variable => variable.Name == parameter.Name);
                if (type == null)
                {
                    throw new InvalidOperationException($"Function parameter '{parameter.Name}' not found.");
                }
                var value = GetValue(environment, parameter.Value, type);
                result.Add(parameter.Name, value);
            }
            return result;
        }

        private object GetValue(ExecutionEnvironment environment, string value, VariableDefinition definition)
        {
            return TypeConverter.Convert(environment, value, definition.Type);
        }

        public string ParserLogging()
        {
            return parserContext.FormatMessages();
        }
    }
}