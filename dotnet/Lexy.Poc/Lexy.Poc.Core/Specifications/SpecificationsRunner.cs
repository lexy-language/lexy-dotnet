using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Specifications
{
    public class SpecificationsRunner
    {
        private readonly LexyParser parser = new LexyParser();

        public void RunAll(string folder)
        {
            var context = new SpecificationRunnerContext();
            var runners = GetRunners(folder, context);
            Console.WriteLine($"Specifications found: {runners.Count}");
            if (runners.Count == 0)
            {
                throw new InvalidOperationException($"No specifications found");
            }

            var lastFile = string.Empty;
            runners.ForEach(runner =>
            {
                if (lastFile != runner.FileName)
                {
                    lastFile = runner.FileName;
                    context.LogGlobal($"{Environment.NewLine}Filename: {runner.FileName}{Environment.NewLine}");
                }

                runner.Run();
            });

            context.LogGlobal($"{Environment.NewLine}Specifications succeed: {runners.Count - context.Failed} / {runners.Count}");

            foreach (var message in context.Messages)
            {
                Console.WriteLine(message);
            }

            Console.WriteLine("--------------- DEBUG LOGGING ---------------");

            foreach (var message in context.DebugMessages)
            {
                Console.WriteLine(message);
            }

            if (context.Failed > 0)
            {
                Failed(runners, context);
            }
        }

        private static void Failed(IList<SpecificationRunner> runners, SpecificationRunnerContext context)
        {
            Console.WriteLine("--------------- FAILED PARSER LOGGING ---------------");
            var logged = new List<string>();
            foreach (var runner in runners.Where(runner => runner.Failed))
            {
                if (runner.Failed && !logged.Contains(runner.FileName))
                {
                    Console.WriteLine($"------- Filename: {runner.FileName}");
                    logged.Add(runner.FileName);
                    Console.WriteLine(runner.ParserLogging());
                }
            }

            throw new InvalidOperationException($"Specifications failed: {context.Failed}");
        }

        private IList<SpecificationRunner> GetRunners(string folder,
            SpecificationRunnerContext runnerContext)
        {
            var absoluteFolder = GetAbsoluteFolder(folder);

            Console.WriteLine($"Specifications folder: {absoluteFolder}");

            var result = new List<SpecificationRunner>();
            AddSpecifications(result, absoluteFolder, runnerContext);
            return result;
        }

        private void AddSpecifications(List<SpecificationRunner> result, string folder,
            SpecificationRunnerContext runnerContext)
        {
            var files = Directory.GetFiles(folder, "*.lexy");
            files
                .OrderBy(name => name)
                .SelectMany(file => ParseFile(file, runnerContext))
                .ForEach(result.Add);

            Directory.GetDirectories(folder)
                .OrderBy(name => name)
                .ForEach(folder => AddSpecifications(result, folder, runnerContext));
        }

        private IEnumerable<SpecificationRunner> ParseFile(string fileName,
            SpecificationRunnerContext runnerContext)
        {
            var parserContext = parser.ParseFile(fileName, false);

            return parserContext
                .Components
                .GetScenarios()
                .Select(scenario => SpecificationRunner.Create(fileName, scenario, runnerContext, parserContext));
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
}