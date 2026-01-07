using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class SpreadAssignmentExpression : Expression
{
    public Expression Assignment { get; }
    public VariablesMapping Mapping { get; private set; }

    private SpreadAssignmentExpression(Expression assignment, ExpressionSource source,
        SourceReference reference) : base(source, reference)
    {
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");

        var assignment = factory.Parse(tokens.TokensFrom(2), source.Line);
        if (!assignment.IsSuccess) return assignment;

        var reference = source.CreateReference();

        var expression = new SpreadAssignmentExpression(assignment.Result, source, reference);

        return ParseExpressionResult.Success(expression);
    }

    public static bool IsValid(TokenList tokens)
    {
        return tokens.Length >= 2
            && tokens.IsOperatorToken(0, OperatorType.Spread)
            && tokens.IsOperatorToken(1, OperatorType.Assignment);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Assignment;
    }

    protected override void Validate(IValidationContext context)
    {
        var expressionType = Assignment.DeriveType(context);
        if (expressionType is GeneratedType objectResultsType)
        {
            Mapping = ExtractResultsFunctionExpression.GetMapping(Reference, context, objectResultsType);
        }
        else
        {
            context.Logger.Fail(Reference, "Couldn't determine type of assignment.");
        }
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        return Assignment.DeriveType(context);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return Assignment.GetReadVariableUsage();
    }
}