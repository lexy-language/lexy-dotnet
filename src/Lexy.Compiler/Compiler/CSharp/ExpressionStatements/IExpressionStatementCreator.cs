using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatements;

internal interface IExpressionStatementCreator<TExpression> where TExpression: Expression
{
    bool Matches(TExpression expression);

    IEnumerable<StatementSyntax> CreateExpressionSyntax(TExpression expression);
}