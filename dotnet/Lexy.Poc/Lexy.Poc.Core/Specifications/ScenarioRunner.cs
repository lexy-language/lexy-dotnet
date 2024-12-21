using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.RunTime;
using Microsoft.Extensions.DependencyInjection;

namespace Lexy.Poc.Core.Specifications
{
    public class ScenarioRunner : IScenarioRunner, IDisposable
    {
        private ISpecificationRunnerContext context;
        private readonly ILexyCompiler lexyCompiler;

        private string fileName;
        private Scenario scenario;
        private Function function;
        private Components components;
        private IParserLogger parserLogger;
        private IServiceScope serviceScope;

        public bool Failed { get; private set; }
        public Scenario Scenario => scenario;

        public ScenarioRunner(ILexyCompiler lexyCompiler)
        {
            this.lexyCompiler = lexyCompiler;
        }

        public void Initialize(string fileName, Components components, Scenario scenario,
            ISpecificationRunnerContext context, IServiceScope serviceScope, IParserLogger parserLogger)
        {
            //parserContext and runnerContext are managed by a parent ServiceProvider scope,
            //thus they can't be injected via the constructor
            if (this.fileName != null)
            {
                throw new InvalidOperationException(
                    "Initialize should only called once. Scope should be managed by ServiceProvider.CreateScope");
            }

            this.fileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            this.context = context;

            this.components = components ?? throw new ArgumentNullException(nameof(components));
            this.scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            this.parserLogger = parserLogger ?? throw new ArgumentNullException(nameof(parserLogger));
            this.serviceScope = serviceScope ?? throw new ArgumentNullException(nameof(serviceScope));

            function = scenario.Function ?? components.GetFunction(scenario.FunctionName.Value);
        }

        public static IScenarioRunner Create(string fileName, Scenario scenario,
            IParserContext parserContext, ISpecificationRunnerContext context,
            IServiceProvider serviceProvider)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));

            var serviceScope = serviceProvider.CreateScope();
            var scenarioRunner = serviceScope.ServiceProvider.GetRequiredService<IScenarioRunner>();
            scenarioRunner.Initialize(fileName, parserContext.Components, scenario, context, serviceScope, parserContext.Logger);

            return scenarioRunner;
        }

        public void Run()
        {
            if (parserLogger.ComponentHasErrors(scenario))
            {
                Fail($"  Parsing scenario failed: {scenario.FunctionName}");
                parserLogger.ComponentFailedMessages(scenario).ForEach(context.Log);
                return;
            }

            if (function == null && !scenario.ExpectRootErrors.HasValues)
            {
                Fail($"  Function not found: {scenario.FunctionName}");
                return;
            }

            if (!ValidateErrors(context)) return;

            var compilerResult = lexyCompiler.Compile(components, function);
            var executable = compilerResult.GetFunction(function);
            var values = GetValues(scenario.Parameters, function.Parameters, compilerResult);

            var result = executable.Run(values);

            var validationResultText = GetValidationResult(result, compilerResult);
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

        private string GetValidationResult(FunctionResult result, CompilerResult compilerResult)
        {
            var validationResult = new StringWriter();
            foreach (var expected in scenario.Results.Assignments)
            {
                var actual = result[expected.Name];

                var expectedValue = TypeConverter.Convert(compilerResult, expected.Expression.ToString(),
                    function.Results.GetParameterType(expected.Name));

                if (Comparer.Default.Compare(actual, expectedValue) != 0)
                {
                    validationResult.WriteLine($"'{expected.Name}' should be '{expectedValue}' but is '{actual}'");
                }
            }

            return validationResult.ToString();
        }

        private bool ValidateErrors(ISpecificationRunnerContext context)
        {
            return ValidateFunctionErrors(context);
        }

        private bool ValidateFunctionErrors(ISpecificationRunnerContext context)
        {
            if (scenario.ExpectRootErrors.HasValues)
            {
                return ValidateRootErrors(context);
            }
            var failedMessages = parserLogger.ComponentFailedMessages(function);

            if (failedMessages.Length > 0 && !scenario.ExpectError.HasValue)
            {
                Fail("Exception occured: " + failedMessages.Format());
                context.Fail(scenario, "Exception occured: " + failedMessages.Format());
                return false;
            }

            if (!scenario.ExpectError.HasValue) return true;

            if (failedMessages.Any(message => message.Contains(scenario.ExpectError.Message)))
            {
                context.Success(scenario);
                return false;
            }

            Fail($"Wrong exception {Environment.NewLine}  Expected: {scenario.ExpectError.Message}{Environment.NewLine}  Actual: {failedMessages.Format()}");
            return false;
        }

        private bool ValidateRootErrors(ISpecificationRunnerContext specificationRunnerContext)
        {
            var failedMessages = parserLogger.FailedRootMessages().ToList();

            var failed = false;
            foreach (var rootMessage in scenario.ExpectRootErrors.Messages)
            {
                var failedMessage = failedMessages.Find(message => message.Contains(rootMessage));
                if (failedMessage != null)
                {
                    failedMessages.Remove(failedMessage);
                }
                else
                {
                    failed = true;
                }
            }

            if (!failedMessages.Any() && !failed)
            {
                context.Success(scenario);
                return false; // don't compile and run rest of scenario
            }

            Fail($"Wrong exception {Environment.NewLine}  Expected: {scenario.ExpectRootErrors.Messages.Format()}{Environment.NewLine}  Actual: {parserLogger.FailedRootMessages().Format()}");
            return false;
        }

        private IDictionary<string, object> GetValues(ScenarioParameters scenarioParameters,
            FunctionParameters functionParameters, CompilerResult compilerResult)
        {
            var result = new Dictionary<string, object>();
            foreach (var parameter in scenarioParameters.Assignments)
            {
                var type = functionParameters.Variables.FirstOrDefault(variable => variable.Name == parameter.Name);
                if (type == null)
                {
                    throw new InvalidOperationException($"Function '{function.ComponentName}' parameter '{parameter.Name}' not found.");
                }
                var value = GetValue(compilerResult, parameter.Expression.ToString(), type);
                result.Add(parameter.Name, value);
            }
            return result;
        }

        private object GetValue(CompilerResult compilerResult, string value, VariableDefinition definition)
        {
            return TypeConverter.Convert(compilerResult, value, definition.Type);
        }

        public string ParserLogging()
        {
            return $"------- Filename: {fileName}{Environment.NewLine}{parserLogger.FormatMessages()}";
        }

        public void Dispose()
        {
            serviceScope?.Dispose();
            serviceScope = null;
        }
    }
}