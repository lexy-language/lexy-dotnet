using System;
using System.Linq;
using Lexy.Compiler.Compiler;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Specifications;

public class SpecificationsRunner : ISpecificationsRunner
{
    private readonly ILexyParser parser;
    private readonly IFileSystem fileSystem;
    private readonly ILexyCompiler compiler;
    private readonly ILogger<SpecificationsRunner> logger;

    public SpecificationsRunner(ILexyParser parser, IFileSystem fileSystem, ILexyCompiler compiler, ILogger<SpecificationsRunner> logger)
    {
        this.parser = parser ?? throw new ArgumentNullException(nameof(parser));
        this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        this.compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Run(string file)
    {
        var context = new SpecificationRunnerContext(logger);

        CreateFileRunner(file, context);
        RunScenarios(context);
    }

    public void RunAll(string folder)
    {
        var context = new SpecificationRunnerContext(logger);

        GetRunners(folder, context);
        RunScenarios(context);
    }

    private static void RunScenarios(ISpecificationRunnerContext context)
    {
        var runners = context.FileRunners;
        var countScenarios = context.CountScenarios();
        Console.WriteLine($"Specifications found: {countScenarios}");
        if (runners.Count == 0) throw new InvalidOperationException("No specifications found");

        runners.ForEach(runner => runner.Run());

        context.LogGlobal($"Specifications succeed: {countScenarios - context.Failed} / {countScenarios}");
        context.LogTimeSpent();

        if (context.Failed > 0) Failed(context);
    }

    private static void Failed(ISpecificationRunnerContext context)
    {
        Console.WriteLine("--------------- FAILED PARSER LOGGING ---------------");
        foreach (var runner in context.FailedScenariosRunners())
        {
            Console.WriteLine(runner.ParserLogging());
        }

        throw new InvalidOperationException($"Specifications failed: {context.Failed}");
    }

    private void GetRunners(string folder, ISpecificationRunnerContext context)
    {
        var absoluteFolder = GetAbsoluteFolder(folder);

        Console.WriteLine($"Specifications folder: {absoluteFolder}");

        AddFolder(absoluteFolder, context);
    }

    private void AddFolder(string folder, ISpecificationRunnerContext context)
    {
        var files = this.fileSystem.GetDirectoryFiles(folder, new []{
            $".{LexySourceDocument.FileExtension}",
            $".{LexySourceDocument.MarkdownExtension}"
        });

        files
            .OrderBy(name => name)
            .ForEach(file => CreateFileRunner(file, context));

        fileSystem.GetDirectories(folder)
            .OrderBy(name => name)
            .ForEach(folder => AddFolder(folder, context));
    }

    private void CreateFileRunner(string fileName, ISpecificationRunnerContext context)
    {
        var runner = new SpecificationFileRunner(fileName, parser, context, compiler);
        runner.Initialize();
        context.Add(runner);
    }

    private string GetAbsoluteFolder(string folder)
    {
        var absoluteFolder = fileSystem.IsPathRooted(folder)
            ? folder
            : fileSystem.GetFullPath(folder);

        if (!fileSystem.DirectoryExists(absoluteFolder))
        {
            throw new InvalidOperationException($"Specifications folder doesn't exist: {absoluteFolder}");
        }

        return absoluteFolder;
    }
}