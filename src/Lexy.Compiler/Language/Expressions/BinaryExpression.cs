using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Compiler.Language.Symbols;
using Lexy.Compiler.Language.TypeSystem;
using Lexy.Compiler.Parser.Context;
using Lexy.Compiler.Parser.Tokens;
using Lexy.RunTime;
using Type = Lexy.Compiler.Language.TypeSystem.Type;
using ValueType = Lexy.Compiler.Language.TypeSystem.ValueType;

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

    private class OperatorCombination
    {
        private readonly Type leftType;
        private readonly Type rightType;

        private readonly bool leftTypeEnum;
        private readonly bool rightTypeEnum;

        private Type LeftType
        {
            get
            {
                if (leftTypeEnum) throw new InvalidOperationException("Left type is enum");
                return leftType;
            }
        }

        private Type RightType
        {
            get
            {
                if (rightTypeEnum) throw new InvalidOperationException("Right type is enum");
                return rightType;
            }
        }

        public ExpressionOperator ExpressionOperator { get; }

        private OperatorCombination(bool leftTypeEnum, Type leftType, bool rightTypeEnum, Type rightType, ExpressionOperator expressionOperator)
        {
            this.leftTypeEnum = leftTypeEnum;
            this.leftType = leftType;
            this.rightTypeEnum = rightTypeEnum;
            this.rightType = rightType;
            ExpressionOperator = expressionOperator;
        }

        public OperatorCombination(Type leftType, Type rightType, ExpressionOperator expressionOperator)
        {
            this.leftType = leftType;
            this.rightType = rightType;
            ExpressionOperator = expressionOperator;
        }

        public static OperatorCombination Enums(ExpressionOperator expressionOperator)
        {
            return new OperatorCombination(true, null, true, null, expressionOperator);
        }

        public static OperatorCombination RightEnum(Type leftType, ExpressionOperator expressionOperator)
        {
            return new OperatorCombination(false, leftType, true, null, expressionOperator);
        }

        public bool Allowed(Type left, Type right)
        {
            var leftEnum = left is EnumType;
            var rightEnum = right is EnumType;

            if (leftTypeEnum && rightTypeEnum)
            {
                //if left and right is enum, the enum should be of the same type
                return leftEnum && rightEnum && left.Equals(right);
            }
            if (leftTypeEnum)
            {
                return leftEnum && RightType.Equals(right);
            }
            if (rightTypeEnum)
            {
                return LeftType.Equals(left) && rightEnum;
            }
            return LeftType.Equals(left) && RightType.Equals(right);

        }
    }

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
        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.Equals),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Equals),
        new OperatorCombination(ValueType.Boolean, ValueType.Boolean, ExpressionOperator.Equals),
        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.Equals),
        OperatorCombination.Enums(ExpressionOperator.Equals),

        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.NotEqual),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.NotEqual),
        new OperatorCombination(ValueType.Boolean, ValueType.Boolean, ExpressionOperator.NotEqual),
        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.NotEqual),
        OperatorCombination.Enums(ExpressionOperator.NotEqual),

        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.Addition),
        new OperatorCombination(ValueType.String, ValueType.Number, ExpressionOperator.Addition),
        new OperatorCombination(ValueType.String, ValueType.Boolean, ExpressionOperator.Addition),
        new OperatorCombination(ValueType.String, ValueType.Date, ExpressionOperator.Addition),
        OperatorCombination.RightEnum(ValueType.String, ExpressionOperator.Addition),

        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Addition),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Subtraction),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Multiplication),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Division),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.Modulus),

        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.GreaterThan),
        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.LessThan),
        new OperatorCombination(ValueType.String, ValueType.String, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.GreaterThan),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.LessThan),
        new OperatorCombination(ValueType.Number, ValueType.Number, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.GreaterThan),
        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.GreaterThanOrEqual),
        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.LessThan),
        new OperatorCombination(ValueType.Date, ValueType.Date, ExpressionOperator.LessThanOrEqual),

        new OperatorCombination(ValueType.Boolean, ValueType.Boolean, ExpressionOperator.And),
        new OperatorCombination(ValueType.Boolean, ValueType.Boolean, ExpressionOperator.Or),
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

    public BinaryState State { get; private set; }

    public BinaryState StateRequired
    {
        get
        {
            if (State == null) throw new InvalidOperationException("State not set.");
            return State;
        }
    }

    private BinaryExpression(Expression left, Expression right, ExpressionOperator operatorValue,
        ExpressionSource source, NodeReference parentReference, SourceReference reference) :
        base(source, parentReference, reference)
    {
        Left = left;
        Right = right;
        Operator = operatorValue;
    }

    public static ParseExpressionResult Parse(ExpressionSource source, NodeReference parentReference, IExpressionFactory factory)
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

        var expressionReference = new NodeReference();
        var left = factory.Parse(expressionReference, leftTokens, source.Line);
        if (!left.IsSuccess) return left;

        var right = factory.Parse(expressionReference, rightTokens, source.Line);
        if (!right.IsSuccess) return left;

        var operatorValue = lowestPriorityOperation.ExpressionOperator;
        var reference = source.CreateReference();

        var binaryExpression = new BinaryExpression(left.Result, right.Result, operatorValue, source, parentReference, reference);
        expressionReference.SetNode(binaryExpression);

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
        var leftType = Left.DeriveType(context);
        var rightType = Right.DeriveType(context);

        State = new BinaryState(leftType, rightType);

        if (leftType == null || rightType == null)
        {
            context.Logger.Fail(Reference,
                $"Invalid operator '{Operator}'. Can't derive type.");
            return;
        }

        if (!IsAllowedOperation(leftType, rightType))
        {
            context.Logger.Fail(Reference,
                $"Invalid operator '{Operator}'. Left type: '{leftType}' and right type '{rightType}' not supported.");
        }
    }

    private bool IsAllowedOperation(Type left, Type right)
    {
        return AllowedOperationCombinations.Any(combination =>
        {
            if (combination.ExpressionOperator != Operator) return false;

            return combination.Allowed(left, right);
        });
    }

    public override Type DeriveType(IValidationContext context)
    {
        if (ComparisonOperators.Contains(Operator))
        {
            return ValueType.Boolean;
        }

        var left = Left.DeriveType(context);
        var right = Right.DeriveType(context);

        return left.Equals(right) ? left : null;
    }

    public override Symbol GetSymbol() => null;
}
