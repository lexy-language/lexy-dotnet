using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class AssignmentExpression : Expression
{
    public Expression Variable { get; }
    public Expression Assignment { get; }

    private AssignmentExpression(Expression variable, Expression assignment, ExpressionSource source,
        NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        Variable = variable;
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var expressionReference = new NodeReference();

        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");

        var variableExpression = factory.Parse(expressionReference, tokens.TokensFromStart(1), source.Line);
        if (!variableExpression.IsSuccess) return variableExpression;

        var assignment = factory.Parse(expressionReference, tokens.TokensFrom(2), source.Line);
        if (!assignment.IsSuccess) return assignment;

        var reference = source.CreateReference();

        var expression = new AssignmentExpression(variableExpression.Result, assignment.Result, source, parentReference, reference);
        expressionReference.SetNode(expression);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length >= 3
            && (tokens.IsTokenType<StringLiteralToken>(0) || tokens.IsTokenType<MemberAccessToken>(0))
            && tokens.IsOperatorToken(1, OperatorType.Assignment);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Variable;
        yield return Assignment;
    }

    protected override void Validate(IValidationContext context)
    {
        var hasVariableReference = Variable as IHasVariableReference;
        if (hasVariableReference?.Variable == null)
        {
            var path = hasVariableReference?.Path ?? Variable.ToString();
            context.Logger.Fail(Reference, $"Unknown variable name: '{path}'.");
            return;
        }

        var variableReference = hasVariableReference.Variable;
        var expressionType = Assignment.DeriveType(context);
        if (expressionType != null && !variableReference.Type.Equals(expressionType))
        {
            context.Logger.Fail(Reference,
                $"Variable '{variableReference}' of type '{variableReference.Type}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public override Type DeriveType(IValidationContext context)
    {
        return Assignment.DeriveType(context);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        if (Variable is not IHasVariableReference hasVariableReference
         || hasVariableReference.Variable == null)
        {
            return Assignment.GetReadVariableUsage();
        }

        var assignmentVariable = hasVariableReference.Variable;
        var writeVariableUsage = new VariableUsage(
            Reference,
            assignmentVariable.Path,
            assignmentVariable.ComponentType,
            assignmentVariable.Type,
            assignmentVariable.Source,
            VariableAccess.Write);

        return new [] { writeVariableUsage }
            .Union(Assignment.GetReadVariableUsage());
    }

    public override Symbol GetSymbol() => null;
}
