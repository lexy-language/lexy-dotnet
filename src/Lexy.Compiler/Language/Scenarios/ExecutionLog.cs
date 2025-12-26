using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class ExecutionLog : ParsableNode
{
    private readonly List<ExecutionLog> entries = new();
    private readonly List<IAssignmentDefinition> assignments = new();

    public string Message { get; }
    public IReadOnlyList<ExecutionLog> Entries => entries;

    public IReadOnlyList<IAssignmentDefinition> Assignments => assignments;


    private ExecutionLog(string message, SourceReference reference) : base(reference)
    {
        Message = message;
    }

    public static ExecutionLog ParseLog(IParseLineContext context)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var reference = line.LineStartReference();
        if (!tokens.IsKeyword(0, Keywords.ExecutionLog))
        {
            context.Logger.Fail(line.LineStartReference(), "Keyword expected 'Log'");
            return null;
        }

        if (tokens.Length != 2)
        {
            context.Logger.Fail(line.LineStartReference(),
                $"Invalid number of tokens '{tokens.Length}'. Expected: '2'");
            return null;
        }

        var token = context.Line.Tokens[1];

        if (token is not QuotedLiteralToken messageToken)
        {
            context.Logger.Fail(line.TokenReference(1), "Invalid token. \"Message\" expected.");
            return null;
        }

        return new ExecutionLog(messageToken.Value, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        if (!context.Line.Tokens.IsKeyword(0, Keywords.ExecutionLog))
        {
            return ParseAssignment(context);
        }

        return ParseEntry(context);
    }

    private IParsableNode ParseEntry(IParseLineContext context)
    {
        var entry = ParseLog(context);
        if (entry == null) return this;
        entries.Add(entry);
        return entry;
    }

    private IParsableNode ParseAssignment(IParseLineContext context)
    {
        var assignment = AssignmentDefinitionParser.Parse(context);
        if (assignment == null) return this;
        assignments.Add(assignment);

        return assignment is IParsableNode parsableNode ? parsableNode : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        //assignments should not be validated
        return entries;
    }

    protected override void Validate(IValidationContext context)
    {
    }
}