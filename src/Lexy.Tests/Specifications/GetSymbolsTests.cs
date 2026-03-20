using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lexy.Compiler.Infrastructure;
using Lexy.Compiler.Language;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Logging;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Tests.Symbols;
using NUnit.Framework;

namespace Lexy.Tests.Specifications;

public class GetSymbolsTests : ScopedServicesTestFixture
{
    private readonly FileSystem fileSystem = new();

    private const string BaseFolder = "../../../lexy-language/Specifications/Symbols/";

    private readonly IProject project = CreateProject();

    private static IProject CreateProject() => new Project(BaseFolder, new FileSystem());

    [Test]
    public async Task AllKeywords() => await Verify("AllKeywords.lexy");

    [Test]
    public async Task Enum() => await Verify("Enum.lexy");

    [Test]
    public async Task Function() => await Verify("Function.lexy");

    [Test]
    public async Task Table() => await Verify("Table.lexy");

    [Test]
    public async Task Type() => await Verify("Type.lexy");

    [Test]
    public async Task SystemFunctions() => await Verify("SystemFunctions.lexy");

    private async Task Verify(string fileName)
    {
        var file = project.File(fileName);
        await Tests.Verify.All(async context => await VerifyCaseFile(context, file));
    }

    private async Task VerifyCaseFile(VerifyContext context, IFile file)
    {
        context.Log("File: " + file.Name);

        var result = await ServiceProvider.GetSymbols(file);

        await VerifyNodes(context, result.File, result.Nodes);
        await VerifySymbols(context, result.File, result.Symbols);
    }

    private async Task VerifyNodes(VerifyContext context, IFile file, ComponentNodeList nodes)
    {
        context.Log("> Nodes:");

        var nodesLogFile = file.FullPath.Replace(".lexy", ".nodes");
        var nodesLog = await ReadNodesLog(context, nodesLogFile);
        var (failed, log) = VerifyNodesFile(context, nodes, nodesLog);

        if (failed)
        {
            var expectedFileName = nodesLogFile + ".actual";
            await fileSystem.WriteAllLines(expectedFileName, log);
            context.Log($"  - Expected saved: " + expectedFileName);
        }
    }

    private async Task<string[]> ReadNodesLog(VerifyContext context, string nodesLogFile)
    {
        if (await fileSystem.FileExists(nodesLogFile))
        {
            return await fileSystem.ReadAllLines(nodesLogFile);
        }

        context.Fail($"\n  - File not found: " + nodesLogFile);
        return Array.Empty<string>();
    }

    private static Tuple<bool, List<string>> VerifyNodesFile(VerifyContext context, ComponentNodeList nodes, string[] nodesLog)
    {
        var log = new List<string>();

        NodesLogger.Log(nodes, value => log.Add(value));

        var failed = false;
        for (var index = 0; index < nodesLog.Length; index++)
        {
            if (VerifyNode(context, nodesLog, index, log))
            {
                failed = true;
            }
        }

        if (nodesLog.Length != log.Count)
        {
            context.Fail($"\n  - Invalid node log length: Actual: {log.Count} Expected: {nodesLog.Length}");
            return new Tuple<bool, List<string>>(true, log);
        }

        return new Tuple<bool, List<string>>(failed, log);
    }

    private static bool VerifyNode(VerifyContext context, string[] nodesLog, int index, List<string> log)
    {
        var expectedLog = nodesLog[index];
        var actualLog = index < log.Count ? log[index] : null;

        if (expectedLog != actualLog)
        {
            context.Fail($"\n  - Invalid node log: {index}\n    Expect: {expectedLog}\n    Actual: {actualLog}");
            return true;
        }

        return false;
    }

    private async Task VerifySymbols(VerifyContext context, IFile file, ISymbols symbols)
    {
        context.Log("> Symbols:");

        var documentSymbols = symbols.Document(file);

        var expectedSymbolsFile = file.FullPath.Replace(".lexy", ".symbols");
        var expectedSymbolsLines = await ReadSymbolsFile(context, expectedSymbolsFile);

        var failed = expectedSymbolsLines == null || VerifySymbolsLog(context, expectedSymbolsLines, documentSymbols);

        if (failed)
        {
            context.Fail("\n  - Invalid symbols");

            var expectedFileName = expectedSymbolsFile + ".actual";
            var expectedSymbols = CreateSymbols(documentSymbols);

            await fileSystem.WriteAllLines(expectedFileName, expectedSymbols);

            context.Log("  - Expected saved: " + expectedFileName);
        }
    }

    private static bool VerifySymbolsLog(VerifyContext context, string[] expectedSymbolsLines,
        IDocumentSymbols documentSymbols)
    {
        var failed = false;
        for (var index = 0; index < expectedSymbolsLines.Length; index++)
        {
            var expectedSymbolsLine = expectedSymbolsLines[index];
            var expectedSymbol = ExpectedSymbol.Parse(index, expectedSymbolsLine);
            if (expectedSymbol != null && !expectedSymbol.Verify(documentSymbols, context))
            {
                failed = true;
            }
        }

        return failed;
    }

    private async Task<string[]> ReadSymbolsFile(VerifyContext context, string expectedSymbolsFile)
    {
        if (!await fileSystem.FileExists(expectedSymbolsFile))
        {
            context.Fail($"\n  - File not found: " + expectedSymbolsFile);
            return null;
        }
        return await fileSystem.ReadAllLines(expectedSymbolsFile);
    }

    private IEnumerable<string> CreateSymbols(IDocumentSymbols symbols)
    {
        var log = new List<string>();
        symbols.WalkSymbols((node, symbol) =>
        {
            var lineNumber = symbol.Reference.LineNumber;
            var column = GetColumn(node, symbol);
            if (column == null) return;

            var label = symbol.Name;
            var description = symbol.Description;
            var expected = string.IsNullOrEmpty(description)
                ? $"{lineNumber}, {column}, \"{Escape(label)}\", SymbolKind.{symbol.Kind}"
                : $"{lineNumber}, {column}, \"{Escape(label)}\", SymbolKind.{symbol.Kind}, \"{Escape(description)}\"";

            log.Add(expected);
        });

        return log;
    }

    private static string Escape(string value)
    {
        return value.ReplaceLineEndings("\\n")
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"");
    }

    private static int? GetColumn(INode node, Symbol symbol)
    {
        var range = new CodeRange(symbol);
        SubtractChildren(range, node.GetChildren());
        return range.Random();
    }

    private static void SubtractChildren(CodeRange range, IEnumerable<INode> children)
    {
        foreach (var child in children)
        {
            if (child.GetSymbol() != null)
            {
                range.Subtract(child.Reference);
            }
            SubtractChildren(range, child.GetChildren());
        }
    }
}
