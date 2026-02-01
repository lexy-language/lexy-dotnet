using System.Collections.Generic;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public abstract class ErrorsNode<TNode> : ParsableNode
{
    private readonly IList<string> messages = new List<string>();

    public IEnumerable<string> Messages => messages;

    public bool HasValues => messages.Count > 0;

    protected ErrorsNode(NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var line = context.Line;
        var valid = context.ValidateTokens<TNode>()
            .Count(1)
            .QuotedString(0)
            .IsValid;

        if (!valid) return this;

        messages.Add(line.Tokens.Token<QuotedLiteralToken>(0).Value);
        return this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol() => null;
}
