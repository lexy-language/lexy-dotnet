using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class AssignmentExpression : Expression
{
    public Expression Variable { get; }
    public Expression Assignment { get; }

    private AssignmentExpression(Expression variable, Expression assignment, ExpressionSource source,
        SourceReference reference) : base(source, reference)
    {
        Variable = variable;
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");

        var variableExpression = factory.Parse(tokens.TokensFromStart(1), source.Line);
        if (!variableExpression.IsSuccess) return variableExpression;

        var assignment = factory.Parse(tokens.TokensFrom(2), source.Line);
        if (!assignment.IsSuccess) return assignment;

        var reference = source.CreateReference();

        var expression = new AssignmentExpression(variableExpression.Result, assignment.Result, source, reference);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length >= 3
               && (tokens.IsTokenType<StringLiteralToken>(0) || tokens.IsTokenType<MemberAccessLiteralToken>(0))
               && tokens.IsOperatorToken(1, OperatorType.Assignment);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Assignment;
        yield return Variable;
    }

    protected override void Validate(IValidationContext context)
    {
        if (Variable is not IHasVariableReference hasVariableReference
            || hasVariableReference.Variable == null) {
            context.Logger.Fail(Reference, $"Unknown variable name: '{Variable}'.");
            return;
        }

        var variableReference = hasVariableReference.Variable;
        var expressionType = Assignment.DeriveType(context);
        if (expressionType != null && !variableReference.VariableType.Equals(expressionType))
        {
            context.Logger.Fail(Reference,
                $"Variable '{variableReference}' of type '{variableReference.VariableType}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return Assignment.DeriveType(context);
    }

    public override IEnumerable<VariableUsage> UsedVariables() {
        if (Variable is not IHasVariableReference hasVariableReference
            || hasVariableReference.Variable == null)
        {
            return Assignment.GetReadVariableUsage();
        }
        var assignmentVariable = hasVariableReference.Variable;
        var writeVariableUsage = new VariableUsage(assignmentVariable.Path, assignmentVariable.ComponentType, assignmentVariable.VariableType, assignmentVariable.Source, VariableAccess.Write);
        return new [] { writeVariableUsage }
            .Union(Assignment.GetReadVariableUsage());
    }
}