using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.BuiltInFunctions;

internal abstract class FunctionCall<TFunctionExpression> where TFunctionExpression : ExpressionFunction
{
    public virtual MemberDeclarationSyntax CustomMethodSyntax(TFunctionExpression expression)
    {
        return null;
    }

    public abstract ExpressionSyntax CallExpressionSyntax(TFunctionExpression expression);
}