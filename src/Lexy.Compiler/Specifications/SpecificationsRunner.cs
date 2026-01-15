using System;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Generation;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using Lexy.RunTime;
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
        this.parser = Assert.NotNull(parser, nameof(parser));
        this.fileSystem = Assert.NotNull(fileSystem, nameof(fileSystem));
        this.compiler = Assert.NotNull(compiler, nameof(compiler));
        this.logger = Assert.NotNull(logger, nameof(logger));
    }

    public async Task Run(string file)
    {
        var context = new SpecificationRunnerContext(logger);

        await CreateFileRunner(file, context);
        RunScenarios(context);
    }

    public async Task RunAll(string folder)
    {
        var context = new SpecificationRunnerContext(logger);

        await GetRunners(folder, context);
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

    private async Task GetRunners(string folder, ISpecificationRunnerContext context)
    {
        var absoluteFolder = await GetAbsoluteFolder(folder);

        Console.WriteLine($"Specifications folder: {absoluteFolder}");

        await AddFolder(absoluteFolder, context);
    }

    private async Task AddFolder(string folder, ISpecificationRunnerContext context)
    {
        var files = await fileSystem.GetDirectoryFiles(folder, new []{
            $".{LexySourceDocument.FileExtension}",
            $".{LexySourceDocument.MarkdownExtension}"
        });

        foreach (var file in files.OrderBy(name => name))
        {
            await CreateFileRunner(file, context);
        }

        var folders = await fileSystem.GetDirectories(folder);
        foreach (var subFolder in folders.OrderBy(name => name))
        {
            var fullFolder = fileSystem.Combine(folder, subFolder);
            await AddFolder(fullFolder, context);
        }
    }

    private async Task CreateFileRunner(string fileName, ISpecificationRunnerContext context)
    {
        var runner = new SpecificationFileRunner(fileName, parser, context, compiler);
        await runner.Initialize();
        context.Add(runner);
    }

    private async Task<string> GetAbsoluteFolder(string folder)
    {
        var absoluteFolder = fileSystem.IsPathRooted(folder)
            ? folder
            : fileSystem.GetFullPath(folder);

        if (!await fileSystem.DirectoryExists(absoluteFolder))
        {
            throw new InvalidOperationException($"Specifications folder doesn't exist: {absoluteFolder}");
        }

        return absoluteFolder;
    }
}
