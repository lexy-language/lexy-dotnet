using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Parser.Context;

namespace Lexy.Compiler.Language.Functions;

public class FunctionCode : ParsableNode
{
    private readonly ExpressionList expressions;

    public IReadOnlyList<Expression> Expressions => expressions;

    public FunctionCode(Function parent, SourceReference reference) : base(new NodeReference(parent), reference)
    {
        expressions = new ExpressionList(this, reference);
    }

    public override IParsableNode Parse(IParseLineContext context)
    {
        var expression = expressions.Parse(context);
        return expression.IsSuccess && expression.Result is IParsableNode node ? node : this;
    }

    public override IEnumerable<INode> GetChildren()
    {
        return Expressions;
    }

    protected override void Validate(IValidationContext context)
    {
    }

    public override Symbol GetSymbol() => null;

    public override string ToString() => expressions.Count.ToString();
}
