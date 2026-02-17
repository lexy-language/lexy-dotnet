using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser.Context;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public abstract class Expression : Node
{
    public ExpressionSource Source { get; }

    protected Expression(ExpressionSource source, NodeReference parentReference, SourceReference reference) : base(parentReference, reference)
    {
        Source = Assert.NotNull(source, nameof(source));
    }

    public override string ToString()
    {
        var writer = new StringBuilder();
        for (var index = 0; index < Source.Tokens.Length; index++)
        {
            var token = Source.Tokens[index];
            writer.Append(token.Value);
            if (index < Source.Tokens.Length - 1)
            {
                writer.Append(" ");
            }
        }

        return writer.ToString();
    }

    public abstract Type DeriveType(IValidationContext context);

    public virtual IEnumerable<VariableUsage> UsedVariables()
    {
        yield break;
    }
}
