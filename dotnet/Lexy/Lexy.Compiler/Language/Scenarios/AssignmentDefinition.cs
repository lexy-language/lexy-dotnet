using System.Collections.Generic;
using Lexy.Compiler.Language.Expressions;
using Lexy.Compiler.Language.Types;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language
{
    public class AssignmentDefinition : Node
    {
        private readonly Expression variableExpression;
        private readonly Expression valueExpression;

        public ConstantValue ConstantValue { get; }
        public VariableReference Variable { get; }

        public VariableType VariableType { get; private set; }

        private AssignmentDefinition(VariableReference variable, ConstantValue constantValue, Expression variableExpression, Expression valueExpression, SourceReference reference)
            : base(reference)
        {
            Variable = variable;
            ConstantValue = constantValue;

            this.variableExpression = variableExpression;
            this.valueExpression = valueExpression;
        }

        public static AssignmentDefinition Parse(IParserContext context)
        {
            var line = context.CurrentLine;
            var tokens = line.Tokens;
            var reference = context.LineStartReference();

            var assignmentIndex = tokens.Find<OperatorToken>(token => token.Type == OperatorType.Assignment);
            if (assignmentIndex <= 0 || assignmentIndex == tokens.Length -1)
            {
                context.Logger.Fail(reference, "Invalid assignment. Expected: 'Variable = Value'");
                return null;
            }

            var targetExpression = ExpressionFactory.Parse(context.SourceCode.File, tokens.TokensFromStart(assignmentIndex), line);
            if (context.Failed(targetExpression, reference)) return null;

            var valueExpression = ExpressionFactory.Parse(context.SourceCode.File, tokens.TokensFrom(assignmentIndex + 1), line);
            if (context.Failed(valueExpression, reference)) return null;

            var variableReference = VariableReferenceParser.Parse(targetExpression.Result);
            if (context.Failed(variableReference, reference)) return null;

            var constantValue = ConstantValue.Parse(valueExpression.Result);
            if (context.Failed(constantValue, reference)) return null;

            return new AssignmentDefinition(variableReference.Result, constantValue.Result, targetExpression.Result, valueExpression.Result, reference);
        }

        public override IEnumerable<INode> GetChildren()
        {
            yield return variableExpression;
            yield return valueExpression;
        }

        protected override void Validate(IValidationContext context)
        {
            if (!context.VariableContext.Contains(Variable, context))
            {
                //logger by IdentifierExpressionValidation
                return;
            }

            var expressionType = valueExpression.DeriveType(context);

            VariableType = context.VariableContext.GetVariableType(Variable, context);
            if (expressionType != null && !expressionType.Equals(VariableType))
            {
                context.Logger.Fail(Reference, $"Variable '{Variable}' of type '{VariableType}' is not assignable from expression of type '{expressionType}'.");
            }
        }
    }

    public static class VariableReferenceParser
    {

        public static VariableReferenceParseResult Parse(Expression expression)
        {
            return expression switch
            {
                MemberAccessExpression memberAccessExpression => Parse(memberAccessExpression),
                LiteralExpression literalExpression => Parse(literalExpression),
                IdentifierExpression literalExpression => VariableReferenceParseResult.Success(new VariableReference(literalExpression.Identifier)),
                _ => VariableReferenceParseResult.Failed("Invalid constant value. Expected: 'Variable = ConstantValue'")
            };
        }

        private static VariableReferenceParseResult Parse(LiteralExpression literalExpression)
        {
            return literalExpression.Literal switch
            {
                StringLiteralToken stringLiteral => VariableReferenceParseResult.Success(new VariableReference(stringLiteral.Value)),
                _ => VariableReferenceParseResult.Failed("Invalid expression literal. Expected: 'Variable = ConstantValue'")
            };
        }

        private static VariableReferenceParseResult Parse(MemberAccessExpression memberAccessExpression)
        {
            if (memberAccessExpression.MemberAccessLiteral.Parts.Length == 0)
            {
                return VariableReferenceParseResult.Failed("Invalid number of variable reference parts: "
                                                           + memberAccessExpression.MemberAccessLiteral.Parts.Length);
            }

            var variableReference = new VariableReference(memberAccessExpression.MemberAccessLiteral.Parts);
            return VariableReferenceParseResult.Success(variableReference);
        }
    }

    public sealed class VariableReferenceParseResult : ParseResult<VariableReference>
    {
        private VariableReferenceParseResult(VariableReference result) : base(result)
        {
        }

        private VariableReferenceParseResult(bool success, string errorMessage) : base(success, errorMessage)
        {
        }

        public static VariableReferenceParseResult Success(VariableReference result)
        {
            return new VariableReferenceParseResult(result);
        }

        public static VariableReferenceParseResult Failed(string errorMessage)
        {
            return new VariableReferenceParseResult(false, errorMessage);
        }
    }

    public sealed class ConstantValueParseResult : ParseResult<ConstantValue>
    {
        private ConstantValueParseResult(ConstantValue result) : base(result)
        {
        }

        private ConstantValueParseResult(bool success, string errorMessage) : base(success, errorMessage)
        {
        }

        public static ConstantValueParseResult Success(ConstantValue result)
        {
            return new ConstantValueParseResult(result);
        }

        public static ConstantValueParseResult Failed(string errorMessage)
        {
            return new ConstantValueParseResult(false, errorMessage);
        }
    }

    public class ConstantValue
    {
        public object Value { get; }

        private ConstantValue(object value)
        {
            Value = value;
        }

        public static ConstantValueParseResult Parse(Expression expression)
        {
            return expression switch
            {
                LiteralExpression literalExpression => Parse(literalExpression),
                MemberAccessExpression literalExpression => Parse(literalExpression),
                _ => ConstantValueParseResult.Failed("Invalid expression variable. Expected: 'Variable = ConstantValue'")
            };
        }

        private static ConstantValueParseResult Parse(LiteralExpression literalExpression)
        {
            var value = new ConstantValue(literalExpression.Literal.TypedValue);
            return ConstantValueParseResult.Success(value);
        }

        private static ConstantValueParseResult Parse(MemberAccessExpression literalExpression)
        {
            return ConstantValueParseResult.Success(new ConstantValue(literalExpression.MemberAccessLiteral.Value));
        }
    }
}