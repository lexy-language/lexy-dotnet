using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Functions;

public class FunctionCode : ParsableNode
{
    private readonly ExpressionList expressions;

    public IReadOnlyList<Expression> Expressions => expressions;

    public FunctionCode(SourceReference reference, IExpressionFactory factory) : base(reference)
    {
        expressions = new ExpressionList(reference, factory);
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
}