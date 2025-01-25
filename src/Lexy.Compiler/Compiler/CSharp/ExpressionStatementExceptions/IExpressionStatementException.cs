using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Lexy.Compiler.Compiler.CSharp.ExpressionStatementExceptions;

internal interface IExpressionStatementException
{
    bool Matches(Expression expression);

    IEnumerable<StatementSyntax> CallExpressionSyntax(Expression expression);
}