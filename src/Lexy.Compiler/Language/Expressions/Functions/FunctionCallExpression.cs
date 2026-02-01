using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions.Functions;

public abstract class FunctionCallExpression : Expression, INodeWithName
{
    public abstract string Name { get; }

    internal FunctionCallExpression(NodeReference parent, ExpressionSource source)
        : base(source, parent, source.CreateReference())
    {
    }

    public static bool IsValid(TokenList tokens)
    {
        return (tokens.IsTokenType<StringLiteralToken>(0)
             || tokens.IsTokenType<MemberAccessToken>(0))
               && tokens.IsOperatorToken(1, OperatorType.OpenParentheses);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return GetChildren()
            .OfType<Expression>()
            .GetReadVariableUsageNodes();
    }
}
