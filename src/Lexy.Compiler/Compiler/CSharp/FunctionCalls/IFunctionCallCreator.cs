using Lexy.Compiler.Language.Expressions.Functions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.FunctionCalls;

internal interface IFunctionCallCreator<TExpression> where TExpression: FunctionCallExpression
{
    bool Matches(TExpression expression);

    ExpressionSyntax CreateExpressionSyntax(TExpression expression);
}