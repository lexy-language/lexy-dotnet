using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;

namespace Lexy.Compiler.Language.Scenarios;

public class AssignmentDefinition : Node, IAssignmentDefinition
{
    private readonly Expression targetExpression;
    private readonly Expression variableExpression;

    public ConstantValue ConstantValue { get; }
    public VariableReference Variable { get; }

    public VariableType VariableType { get; private set; }

    private AssignmentDefinition(VariableReference variable, ConstantValue constantValue, Expression variableExpression,
        Expression targetExpression, SourceReference reference)
        : base(reference)
    {
        Variable = variable;
        ConstantValue = constantValue;

        this.variableExpression = variableExpression;
        this.targetExpression = targetExpression;
    }

    public static IAssignmentDefinition Parse(IParseLineContext context, VariableReference parentVariable = null)
    {
        var line = context.Line;
        var tokens = line.Tokens;
        var reference = line.LineStartReference();

        var assignmentIndex = tokens.Find<OperatorToken>(token => token.Type == OperatorType.Assignment);
        if (assignmentIndex <= 0)
        {
            context.Logger.Fail(reference, "Invalid assignment. Expected: 'Variable = Value'");
            return null;
        }

        var targetTokens = tokens.TokensFromStart(assignmentIndex);
        if (parentVariable != null) {
            targetTokens = AddParentVariableAccessor(parentVariable, targetTokens);
        }
        var targetExpression = context.ExpressionFactory.Parse(targetTokens, line);
        if (context.Failed(targetExpression, reference)) return null;

        var variableReference = VariableReferenceParser.Parse(targetExpression.Result);
        if (context.Failed(variableReference, reference)) return null;

        if (assignmentIndex == tokens.Length - 1) {
            return new ComplexAssignmentDefinition(variableReference.Result, reference);
        }

        var valueExpression = context.ExpressionFactory.Parse(tokens.TokensFrom(assignmentIndex + 1), line);
        if (context.Failed(valueExpression, reference)) return null;

        var constantValue = ConstantValue.Parse(valueExpression.Result);
        if (context.Failed(constantValue, reference)) return null;

        return new AssignmentDefinition(variableReference.Result, constantValue.Result, targetExpression.Result,
            valueExpression.Result, reference);
    }

    private static TokenList AddParentVariableAccessor(VariableReference parentVariable, TokenList targetTokens)
    {
        if (targetTokens.Length != 1) return targetTokens;
        var variablePath = GetVariablePath(targetTokens);
        if (variablePath == null) return targetTokens;

        var newPath = parentVariable.Append(variablePath.Parts).FullPath();
        var newToken = new MemberAccessLiteral(newPath, variablePath.FirstCharacter);
        return new TokenList(new [] {newToken});
    }

    private record VariablePath(string[] Parts, TokenCharacter FirstCharacter);

    private static VariablePath GetVariablePath(TokenList targetTokens)
    {
        return targetTokens[0] switch
        {
            MemberAccessLiteral memberAccess => new VariablePath(memberAccess.Parts, memberAccess.FirstCharacter),
            StringLiteralToken literal => new VariablePath(new[] { literal.Value }, literal.FirstCharacter),
            _ => null
        };
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return variableExpression;
        yield return targetExpression;
    }

    protected override void Validate(IValidationContext context)
    {
        if (!context.VariableContext.Contains(Variable, context))
        {
            //logger by IdentifierExpressionValidation
            return;
        }

        var expressionType = targetExpression.DeriveType(context);

        VariableType = context.VariableContext.GetVariableType(Variable, context);
        if (expressionType != null && !expressionType.Equals(VariableType))
        {
            context.Logger.Fail(Reference,
                $"Variable '{Variable}' of type '{VariableType}' is not assignable from expression of type '{expressionType}'.");
        }
    }

    public IEnumerable<AssignmentDefinition> Flatten()
    {
        yield return this;
    }
}