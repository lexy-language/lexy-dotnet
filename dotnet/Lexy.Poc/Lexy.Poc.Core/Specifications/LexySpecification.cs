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
    public class SpecificationsRunner
    {
        private readonly LexyParser parser = new LexyParser();

        public void RunAll(string folder)
        {
            var runners = GetRunners(folder);
            Console.WriteLine($"Specifications found: {runners.Count}");
            if (runners.Count == 0)
            {
                throw new InvalidOperationException($"No specifications found");
            }

            var context = new SpecificationRunnerContext();
            runners.ForEach(runner => runner.Run(context));

            foreach (var message in context.Messages)
            {
                Console.WriteLine(message);
            }

            if (context.Failed > 0)
            {
                throw new InvalidOperationException($"Specifications failed: {context.Failed}");
            }

            Console.WriteLine($"Specifications succeed: {runners.Count}");
        }

        private IList<SpecificationRunner> GetRunners(string folder)
        {
            var absoluteFolder = GetAbsoluteFolder(folder);

            Console.WriteLine($"Specifications folder: {absoluteFolder}");

            var result = new List<SpecificationRunner>();
            AddSpecifications(result, absoluteFolder);
            return result;
        }

        private void AddSpecifications(List<SpecificationRunner> result, string folder)
        {
            var files = Directory.GetFiles(folder, "*.lexy");
            Console.WriteLine($"AddSpecifications: {folder} ({files.Length})");

            files
                .OrderBy(name => name)
                .SelectMany(ParseFile)
                .ForEach(result.Add);

            Directory.GetDirectories(folder)
                .OrderBy(name => name)
                .ForEach(folder => AddSpecifications(result, folder));
        }

        private IEnumerable<SpecificationRunner> ParseFile(string fileName)
        {
            var rootTokens = parser.ParseFile(fileName);

            return rootTokens.GetScenarios()
                .Select(token => SpecificationRunner.Create(token, rootTokens));
        }

        private static string GetAbsoluteFolder(string folder)
        {
            var absoluteFolder = Path.IsPathRooted(folder)
                ? folder
                : Path.GetFullPath(folder);
            if (!Directory.Exists(absoluteFolder))
            {
                throw new InvalidOperationException($"Specifications folder doesn't exist: {absoluteFolder}");
            }

            return absoluteFolder;
        }
    }

    public class SpecificationRunner
    {
        private readonly Scenario scenario;
        private readonly Function function;
        private readonly TypeSystem typeSystem;

        private SpecificationRunner(TypeSystem typeSystem, Scenario scenario, Function function)
        {
            this.typeSystem = typeSystem;
            this.scenario = scenario;
            this.function = function;
        }

        public static SpecificationRunner Create(Scenario scenario, TypeSystem typeSystem)
        {
            if (scenario == null) throw new ArgumentNullException(nameof(scenario));
            if (typeSystem == null) throw new ArgumentNullException(nameof(typeSystem));

            var function = typeSystem.GetFunction(scenario.FunctionName.Value);

            return new SpecificationRunner(typeSystem, scenario, function);
        }

        public void Run(SpecificationRunnerContext context)
        {
            if (function == null)
            {
                context.Fail(scenario, $"Function not found: {scenario.FunctionName}");
                return;
            }

            if (function.FailedException != null)
            {
                if (scenario.ExpectError.HasValue)
                {
                    var message = function.FailedException.Message;
                    if (message.StartsWith(scenario.ExpectError.Message))
                    {
                        context.Success(scenario);
                    }
                    else
                    {
                        context.Fail(scenario,
                            $"Wrong exception {Environment.NewLine}  Expected: {scenario.ExpectError.Message}{Environment.NewLine}  Actual: {message}");
                    }
                }
                else
                {
                    context.Fail(scenario, "Exception occured");
                }
                return;
            }


            var compiler = new LexyCompiler();
            var environment = compiler.Compile(typeSystem, function);
            var executable = environment.GetFunction(function);
            var values = GetValues(scenario.Parameters, function.Parameters, environment);
            var result = executable.Run(values);
            var validationResult = new StringWriter();

            foreach (var expected in scenario.Result.Assignments)
            {
                var actual = result[expected.Name];

                var expectedValue = TypeConverter.Convert(environment, expected.Value,
                    function.Result.GetParameterType(expected.Name));

                if (Comparer.Default.Compare(actual, expectedValue) != 0)
                {
                    validationResult.WriteLine($"'{expected.Name}' should be '{expectedValue}' but is '{actual}'");
                }
            }

            var validationResultText = validationResult.ToString();
            if (validationResultText.Length > 0)
            {
                context.Fail(scenario, validationResultText);
            }
            else
            {
                context.Success(scenario);
            }
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
    }

    internal static class TypeConverter
    {
        public static object Convert(ExecutionEnvironment environment, string value, string type)
        {
            if (environment.ContainsEnum(type))
            {
                var indexOfSeparator = value.IndexOf(".");
                var enumValue = value[(indexOfSeparator + 1)..];
                return Enum.Parse(environment.GetEnumType(type), enumValue);
            }

            return type switch
            {
                TypeNames.Int => int.Parse(value),
                TypeNames.Number => decimal.Parse(value),
                TypeNames.DateTime => DateTime.Parse(value),
                TypeNames.Boolean => bool.Parse(value),
                _ => throw new InvalidOperationException($"Invalid type: {type}")
            };
        }
    }


    public class SpecificationRunnerContext
    {
        public IList<string> Messages { get; } = new List<string>();

        public int Failed { get; private set; }

        public void Fail(Scenario scenario, string message)
        {
            Failed++;
            Messages.Add($"- FAILED  - {scenario.Name}: " + message);
        }

        public void Success(Scenario scenario)
        {
            Messages.Add($"- SUCCESS - {scenario.Name}");
        }
    }
}