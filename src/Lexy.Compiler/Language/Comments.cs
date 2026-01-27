using System.Collections.Generic;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Symbols;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language;

public class Comments : ParsableNode
{
    private readonly IList<string> content = new List<string>();

    public Comments(SourceReference sourceReference) : base(sourceReference)
    {
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var valid = context.ValidateTokens<Comments>()
            .Count(1)
            .Comment(0)
            .IsValid;

        if (!valid) return null;

        var comment = context.Line.Tokens.Token<CommentToken>(0);
        content.Add(comment.Value);
        return this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield break;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol()
    {
        return new Symbol(Reference, nameof(Comments), $"//{string.Join("\n", content)}", SymbolKind.Comments);
    }
}
