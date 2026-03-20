using System;
using System.Collections.Generic;
using System.Text;
using Lexy.Compiler.Language.Expressions.Functions.SystemFunctions;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem.Objects;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Type = Lexy.Compiler.Language.TypeSystem.Type;

namespace Lexy.Compiler.Language.Expressions;

public class SpreadAssignmentState
{
    public VariablesMapping Mapping { get; }

    public SpreadAssignmentState(VariablesMapping mapping)
    {
        Mapping = mapping;
    }
}

public class SpreadAssignmentExpression : Expression
{
    public Expression Assignment { get; }

    public SpreadAssignmentState State { get; private set; }

    public SpreadAssignmentState StateRequired
    {
        get
        {
            if (State == null) throw new InvalidOperationException("State not set.");
            return State;
        }
    }

    private SpreadAssignmentExpression(Expression assignment, ExpressionSource source, NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        Assignment = assignment;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference)
    {
        var tokens = source.Tokens;
        if (!IsValid(tokens)) return ParseExpressionResult.Invalid<ParseExpressionResult>("Invalid expression.");

        var expressionReference = new NodeReference();
        var assignment = ExpressionFactory.Parse(expressionReference, tokens.TokensFrom(2), source.Line);
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
        if (expressionType is not GeneratedType objectResultsType)
        {
            context.Logger.Fail(Reference, "Couldn't determine type of assignment.");
            return;
        }

        var mapping = ExtractResultsFunctionExpression.GetMapping(Reference, context, objectResultsType);
        State = new SpreadAssignmentState(mapping);
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
        if (State?.Mapping == null) return null;

        var builder = new StringBuilder();
        foreach (var mapping in State.Mapping)
        {
            if (builder.Length > 0)
            {
                builder.AppendLine();
            }
            builder.Append($"- {mapping.Type} {mapping.VariableName} (from {mapping.VariableSource})");
        }
        var variablesString = builder.ToString();
        return new Symbol(Reference, "operator: spread", variablesString, SymbolKind.Operator);
    }
}
