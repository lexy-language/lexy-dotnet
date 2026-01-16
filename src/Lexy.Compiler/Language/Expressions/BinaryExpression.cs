using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Enums;
using Lexy.Compiler.Language.VariableTypes;
using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;

namespace Lexy.Compiler.Language.Expressions;

public class BinaryExpression : Expression
{
    private class OperatorEntry
    {
        public OperatorType OperatorType { get; }
        public ExpressionOperator ExpressionOperator { get; }

        public OperatorEntry(OperatorType operatorType, ExpressionOperator expressionOperator)
        {
            OperatorType = operatorType;
            ExpressionOperator = expressionOperator;
        }
    }

    private class TokenIndex
    {
        public int Index { get; }
        public OperatorType OperatorType { get; }
        public ExpressionOperator ExpressionOperator { get; }

        public TokenIndex(int index, OperatorType operatorType, ExpressionOperator expressionOperator)
        {
            Index = index;
            OperatorType = operatorType;
            ExpressionOperator = expressionOperator;
        }
    }

    private static VariableType EnumType()
    {
        return new EnumType("*", new EnumDefinition("*", false, new SourceReference(new SourceFile("*"), 1, 1)));
    }

    private record OperatorCombination(VariableType LeftType, VariableType RightType,
        ExpressionOperator ExpressionOperator);

    private static readonly IList<ExpressionOperator> ComparisonOperators = new[]
    {
        ExpressionOperator.GreaterThan,
        ExpressionOperator.GreaterThanOrEqual,
        ExpressionOperator.LessThan,
        ExpressionOperator.LessThanOrEqual,
        ExpressionOperator.Equals,
        ExpressionOperator.NotEqual
    };

    private static readonly IList<OperatorCombination> AllowedOperationCombinations = new[]
    {
        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.Equals),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Equals),
        new OperatorCombination(PrimitiveType.Boolean, PrimitiveType.Boolean, ExpressionOperator.Equals),
        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.Equals),
        new OperatorCombination(EnumType(), EnumType(), ExpressionOperator.Equals),

        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.NotEqual),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.NotEqual),
        new OperatorCombination(PrimitiveType.Boolean, PrimitiveType.Boolean, ExpressionOperator.NotEqual),
        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.NotEqual),
        new OperatorCombination(EnumType(), EnumType(), ExpressionOperator.NotEqual),

        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.Addition),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.Number, ExpressionOperator.Addition),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.Boolean, ExpressionOperator.Addition),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.Date, ExpressionOperator.Addition),
        new OperatorCombination(PrimitiveType.String, EnumType(), ExpressionOperator.Addition),

        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Addition),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Subtraction),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Multiplication),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Division),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.Modulus),

        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.GreaterThan),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.LessThan),
        new OperatorCombination(PrimitiveType.String, PrimitiveType.String, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.GreaterThan),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.LessThan),
        new OperatorCombination(PrimitiveType.Number, PrimitiveType.Number, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.GreaterThan),
        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.LessThan),
        new OperatorCombination(PrimitiveType.Date, PrimitiveType.Date, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(PrimitiveType.Boolean, PrimitiveType.Boolean, ExpressionOperator.And),
        new OperatorCombination(PrimitiveType.Boolean, PrimitiveType.Boolean, ExpressionOperator.Or),
    };

    private static readonly IList<OperatorEntry> SupportedOperatorsByPriority = new List<OperatorEntry>
    {
        new(OperatorType.Multiplication, ExpressionOperator.Multiplication),
        new(OperatorType.Division, ExpressionOperator.Division),
        new(OperatorType.Modulus, ExpressionOperator.Modulus),

        new(OperatorType.Addition, ExpressionOperator.Addition),
        new(OperatorType.Subtraction, ExpressionOperator.Subtraction),

        new(OperatorType.GreaterThan, ExpressionOperator.GreaterThan),
        new(OperatorType.GreaterThanOrEqual, ExpressionOperator.GreaterThanOrEqual),
        new(OperatorType.LessThan, ExpressionOperator.LessThan),
        new(OperatorType.LessThanOrEqual, ExpressionOperator.LessThanOrEqual),

        new(OperatorType.Equals, ExpressionOperator.Equals),
        new(OperatorType.NotEqual, ExpressionOperator.NotEqual),

        new(OperatorType.And, ExpressionOperator.And),
        new(OperatorType.Or, ExpressionOperator.Or)
    };

    public Expression Left { get; }
    public Expression Right { get; }
    public ExpressionOperator Operator { get; }

    public VariableType LeftVariableType { get; private set; }
    public VariableType RightVariableType { get; private set; }

    private BinaryExpression(Expression left, Expression right, ExpressionOperator operatorValue,
        ExpressionSource source, SourceReference reference) : base(source, reference)
    {
        Left = left;
        Right = right;
        Operator = operatorValue;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, IExpressionFactory factory)
    {
        var tokens = source.Tokens;
        var supportedTokens = GetCurrentLevelSupportedTokens(tokens);
        var lowestPriorityOperation = GetLowestPriorityOperation(supportedTokens);
        if (lowestPriorityOperation == null)
        {
            return ParseExpressionResult.Invalid<BinaryExpression>("No valid Operator token found.");
        }

        var leftTokens = tokens.TokensRange(0, lowestPriorityOperation.Index - 1);
        if (leftTokens.Length == 0)
        {
            return ParseExpressionResult.Invalid<BinaryExpression>(
                $"No tokens left from: {lowestPriorityOperation.Index} ({tokens})");
        }

        var rightTokens = tokens.TokensFrom(lowestPriorityOperation.Index + 1);
        if (rightTokens.Length == 0)
        {
            return ParseExpressionResult.Invalid<BinaryExpression>(
                $"No tokens right from: {lowestPriorityOperation.Index} ({tokens})");
        }

        var left = factory.Parse(leftTokens, source.Line);
        if (!left.IsSuccess) return left;

        var right = factory.Parse(rightTokens, source.Line);
        if (!right.IsSuccess) return left;

        var operatorValue = lowestPriorityOperation.ExpressionOperator;
        var reference = source.CreateReference(lowestPriorityOperation.Index);

        var binaryExpression = new BinaryExpression(left.Result, right.Result, operatorValue, source, reference);
        return ParseExpressionResult.Success(binaryExpression);
    }

    private static TokenIndex GetLowestPriorityOperation(IList<TokenIndex> supportedTokens)
    {
        foreach (var supportedOperator in SupportedOperatorsByPriority.Reverse())
        {
            foreach (var supportedToken in supportedTokens)
            {
                if (supportedOperator.OperatorType == supportedToken.OperatorType)
                {
                    return supportedToken;
                }
            }
        }

        return null;
    }

    public static bool IsValid(TokenList tokens)
    {
        var supportedTokens = GetCurrentLevelSupportedTokens(tokens);
        return supportedTokens.Count > 0;
    }

    private static IList<TokenIndex> GetCurrentLevelSupportedTokens(TokenList tokens)
    {
        Assert.NotNull(tokens, nameof(tokens));

        var result = new List<TokenIndex>();
        var countParentheses = 0;
        var countBrackets = 0;
        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index];
            if (!(token is OperatorToken operatorToken)) continue;

            switch (operatorToken.Type)
            {
                case OperatorType.OpenParentheses:
                    countParentheses++;
                    break;
                case OperatorType.CloseParentheses:
                    countParentheses--;
                    break;
                case OperatorType.OpenBrackets:
                    countBrackets++;
                    break;
                case OperatorType.CloseBrackets:
                    countBrackets--;
                    break;
            }

            if (countBrackets != 0 || countParentheses != 0) continue;

            var supported = IsSupported(operatorToken.Type);
            if (supported != null)
            {
                result.Add(new TokenIndex(index, operatorToken.Type, supported.ExpressionOperator));
            }
        }

        return result;
    }

    private static OperatorEntry IsSupported(OperatorType operatorTokenType)
    {
        return SupportedOperatorsByPriority.FirstOrDefault(entry => entry.OperatorType == operatorTokenType);
    }

    public override IEnumerable<INode> GetChildren()
    {
        yield return Left;
        yield return Right;
    }

    protected override void Validate(IValidationContext context)
    {
        LeftVariableType = Left.DeriveType(context);
        RightVariableType = Right.DeriveType(context);
        if (LeftVariableType == null || RightVariableType == null)
        {
            context.Logger.Fail(Reference,
                $"Invalid operator '{Operator}'. Can't derive type.");
            return;
        }

        if (!IsAllowedOperation(LeftVariableType, RightVariableType))
        {
            context.Logger.Fail(Reference,
                $"Invalid operator '{Operator}'. Left type: '{LeftVariableType}' and right type '{RightVariableType}' not supported.");
        }
    }

    private bool IsAllowedOperation(VariableType left, VariableType right)
    {
        return AllowedOperationCombinations.Any(allowed =>
        {
            if (allowed.ExpressionOperator != Operator) return false;

            var leftEnum = left is EnumType;
            var rightEnum = right is EnumType;
            var allowedLeftEnum = allowed.LeftType is EnumType;
            var allowedRightEnum = allowed.RightType is EnumType;

            if (allowedLeftEnum && allowedRightEnum)
            {
                //if left and right is enum, the enum should be of the same type
                return leftEnum && rightEnum && left.Equals(right);
            }
            if (allowedLeftEnum)
            {
                return leftEnum && allowed.RightType.Equals(right);
            }
            if (allowedRightEnum)
            {
                return allowed.LeftType.Equals(left) && rightEnum;
            }

            return allowed.LeftType.Equals(left) && allowed.RightType.Equals(right);
        });
    }

    public override VariableType DeriveType(IValidationContext context)
    {
        if (ComparisonOperators.Contains(Operator))
        {
            return PrimitiveType.Boolean;
        }

        var left = Left.DeriveType(context);
        var right = Right.DeriveType(context);

        return left.Equals(right) ? left : null;
    }
}
