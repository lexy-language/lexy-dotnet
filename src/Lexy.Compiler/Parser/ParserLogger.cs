using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language;
using Microsoft.Extensions.Logging;

namespace Lexy.Compiler.Parser;

public class ParserLogger : IParserLogger
{
    private readonly ILogger logger;
    private readonly IList<LogEntry> logEntries = new List<LogEntry>();

    private IComponentNode currentNode;
    private int failedMessages;

    public ParserLogger(ILogger logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (message == null) throw new ArgumentNullException(nameof(message));

        logger.LogDebug("{Reference}: {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, currentNode, false, $"{reference}: {message}"));
    }

    public void Fail(SourceReference reference, string message)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (message == null) throw new ArgumentNullException(nameof(message));

        failedMessages++;

        logger.LogError("{Reference}: ERROR - {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, currentNode, true, $"{reference}: ERROR - {message}"));
    }

    public void Fail(INode node, SourceReference reference, string message)
    {
        if (reference == null) throw new ArgumentNullException(nameof(reference));
        if (message == null) throw new ArgumentNullException(nameof(message));

        failedMessages++;

        logger.LogError("{Reference}: ERROR - {Message}", reference, message);
        logEntries.Add(new LogEntry(reference, node, true, $"{reference}: ERROR - {message}"));
    }

    public void LogNodes(IEnumerable<INode> nodes)
    {
        if (!logger.IsEnabled(LogLevel.Debug)) return;

        var nodeLogger = new NodesLogger();
        nodeLogger.Log(nodes);

        logger.LogDebug("Parsed nodes: {Nodes}", nodeLogger.ToString());
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
        currentNode = node ?? throw new ArgumentNullException(nameof(node));
    }

    public void ResetCurrentNode()
    {
        currentNode = null;
    }

    public bool NodeHasErrors(IComponentNode node)
    {
        if (node == null) throw new ArgumentNullException(nameof(node));

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