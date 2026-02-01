using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Expressions;

public class SpreadAssignmentExpression : Expression
{
    public Expression Assignment { get; private set; }
    public VariablesMapping Mapping { get; private set; }

    private SpreadAssignmentExpression(Expression assignment, ExpressionSource source, NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");

        var expressionReference = new NodeReference();
        var assignment = factory.Parse(expressionReference, tokens.TokensFrom(2), source.Line);
        if (!assignment.IsSuccess) return null;

        var reference = source.CreateReference();

        var expression = new SpreadAssignmentExpression(assignment.Result, source, parentReference, reference);
        expressionReference.SetNode(expression);

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

    public override Type DeriveType(IValidationContext context)
    {
        return Assignment.DeriveType(context);
    }

    public override IEnumerable<VariableUsage> UsedVariables()
    {
        return Assignment.GetReadVariableUsage();
    }

    public override Symbol GetSymbol()
    {
        var builder = new StringBuilder();
        foreach (var mapping in Mapping)
        {
            builder.AppendLine($"- {mapping.Type} {mapping.VariableName} (from {mapping.VariableSource})");
        }
        var variablesString = builder.ToString();
        return new Symbol(Reference, "spread operator", variablesString, SymbolKind.Operator);
    }
}
