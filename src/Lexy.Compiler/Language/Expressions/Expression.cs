using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public abstract class Expression : Node
{
    public ExpressionSource Source { get; }

    protected Expression(ExpressionSource source, SourceReference reference) : base(reference)
    {
        Source = Assert.NotNull(source, nameof(source));
    }

    public override string ToString()
    {
        var writer = new StringBuilder();
        foreach (var token in Source.Tokens)
        {
            writer.Append(token.Value);
        }
        return writer.ToString();
    }

    public abstract Type DeriveType(IValidationContext context);

    public virtual IEnumerable<VariableUsage> UsedVariables()
    {
        yield break;
    }
}
