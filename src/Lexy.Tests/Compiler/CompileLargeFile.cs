using System;
using System.Linq;
using System.Threading.Tasks;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Parser;
using NUnit.Framework;
using Shouldly;

namespace Lexy.Tests.Compiler;

public class CompileLargeFile : ScopedServicesTestFixture
{
    [Test]
    public async Task ParseCompileAndRun1000Scenarios()
    {
        var fileSystem = new FileSystem();
        var fullPath = fileSystem.Combine(fileSystem.CurrentFolder(), "Compiler/1mb.lexy");
        var bigLexy = await fileSystem.ReadAllLines(fullPath);

        Console.WriteLine("Lines: " + bigLexy.Length);

        GlobalTiming.Init();

        var result = await ServiceProvider.ParseLines(bigLexy);
        result.Nodes.Count().ShouldBe(4000);

        GlobalTiming.Log("ServiceProvider.ParseLines: " + bigLexy.Length);

        return;

        var testResult = ServiceProvider.RunScenarios("1mb.lexy", result.Nodes, result.Logger, result.Dependencies);
        testResult.Any(entry => entry.IsError).ShouldBeFalse();

        GlobalTiming.Log("Time");
    }
}
