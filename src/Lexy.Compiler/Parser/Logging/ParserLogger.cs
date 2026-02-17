using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Lexy.RunTime;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Parser.Logging;

public class ParserLogger : IParserLogger
{
    private readonly ILogger logger;
    private readonly IList<LogEntry> logEntries = new List<LogEntry>();

    private IComponentNode currentNode;
    private int failedMessages;

    public ParserLogger(ILogger logger)
    {
        this.logger = Assert.NotNull(logger, nameof(logger));
    }

    public bool HasErrors()
    {
        return failedMessages > 0;
    }

    public bool HasComponentErrors()
    {
        return logEntries.Any(IsComponentError);
    }

    public void LogInfo(string message)
    {
        logger.LogInformation(message);
    }

    public void Log(SourceReference reference, string message)
    {
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(message, nameof(message));

        logger.LogDebug("{Reference}: {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, currentNode, false, $"{reference}: {message}"));
    }

    public void Fail(SourceReference reference, string message)
    {
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(message, nameof(message));

        failedMessages++;

        logger.LogError("{Reference}: ERROR - {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, currentNode, true, $"{reference}: ERROR - {message}"));
    }

    public void Fail(INode node, SourceReference reference, string message)
    {
        Assert.NotNull(reference, nameof(reference));
        Assert.NotNull(message, nameof(message));

        failedMessages++;

        logger.LogError("{Reference}: ERROR - {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, node, true, $"{reference}: ERROR - {message}"));
    }

    public void LogNodes(IEnumerable<INode> nodes)
    {
        if (!logger.IsEnabled(LogLevel.Debug)) return;

        logger.LogDebug("Parsed nodes:");

        NodesLogger.Log(nodes, value => logger.LogDebug(value));
    }

    public bool HasErrorMessage(string expectedError)
    {
        return logEntries.Any(message => message.IsError && message.Message.Contains(expectedError));
    }

    public string FormatMessages()
    {
        return $"{string.Join(Environment.NewLine, logEntries)}{Environment.NewLine}";
    }

    public void SetCurrentNode(IComponentNode node)
    {
        currentNode = Assert.NotNull(node, nameof(node));
    }

    public void ResetCurrentNode()
    {
        currentNode = null;
    }

    public bool NodeHasErrors(IComponentNode node)
    {
        Assert.NotNull(node, nameof(node));

        return logEntries.Any(message => message.IsError && message.Node == node);
    }

    public string[] ErrorNodeMessages(IComponentNode node)
    {
        return logEntries.Where(entry => entry.IsError && entry.Node == node)
            .OrderBy(entry => entry.SortIndex)
            .Select(entry => entry.Message)
            .ToArray();
    }

    public string[] ErrorNodesMessages(IEnumerable<IComponentNode> nodes)
    {
        return logEntries.Where(entry => entry.IsError && nodes.Contains(entry.Node))
            .OrderBy(entry => entry.SortIndex)
            .Select(entry => entry.Message)
            .ToArray();
    }

    public string[] ErrorComponentMessages()
    {
        return logEntries.Where(IsComponentError)
            .OrderBy(entry => entry.SortIndex)
            .Select(entry => entry.Message)
            .ToArray();
    }

    private static bool IsComponentError(LogEntry entry) => entry.IsError && entry.Node is null or LexyScriptNode;

    public string[] ErrorMessages()
    {
        return logEntries.Where(entry => entry.IsError)
            .OrderBy(entry => entry.SortIndex)
            .Select(entry => entry.Message)
            .ToArray();
    }

    public void AssertNoErrors()
    {
        if (HasErrors()) throw new InvalidOperationException($"Parsing failed: {FormatMessages()}");
    }
}
