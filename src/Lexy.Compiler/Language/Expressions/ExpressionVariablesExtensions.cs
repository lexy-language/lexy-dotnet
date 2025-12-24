using System;
using System.Collections.Generic;

namespace Lexy.Compiler.Language.Expressions;

public static class ExpressionVariablesExtensions
{
    private static void AddVariableExpression(this Expression expression, IList<VariableUsage> results)
    {
        if (expression is not IHasVariableReference hasVariableReference) return;
        var reference = hasVariableReference.Variable;
        if (reference != null)
        {
            var usage = new VariableUsage(reference.Path, reference.ComponentType, reference.VariableType, reference.Source,
                VariableAccess.Read);
            results.Add(usage);
        }
    }

    public static IEnumerable<VariableUsage> GetReadVariableUsageNodes(this IEnumerable<Expression> expressions)
    {
        var results = new List<VariableUsage>();
        NodesWalker.Walk(expressions, node =>
        {
            var expression = node as Expression;
            expression.AddVariableExpression(results);
        });
        return results;
    }

    public static IEnumerable<VariableUsage> GetReadVariableUsage(this Expression expression)
    {
        if (expression == null) return Array.Empty<VariableUsage>();
        var results = new List<VariableUsage>();
        NodesWalker.Walk(expression, node =>
        {
            var expression = node as Expression;
            expression.AddVariableExpression(results);
        });
        return results;
    }
}