using System;

namespace Lexy.Compiler.Parser.Tokens;

public class TokenValidator
{
    private readonly IParserLogger logger;
    private readonly string parserName;
    private readonly TokenList tokens;
    private readonly Line line;

    public bool IsValid { get; private set; }

    public TokenValidator(string parserName, Line line, IParserLogger logger)
    {
        this.parserName = parserName;
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        this.line = line;
        tokens = line.Tokens;

        IsValid = true;
    }

    public TokenValidator Count(int count)
    {
        if (tokens.Length != count)
        {
            Fail(0, $"Invalid number of tokens '{tokens.Length}', should be '{count}'.");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator CountMinimum(int count)
    {
        if (tokens.Length < count)
        {
            Fail(0, $"Invalid number of tokens '{tokens.Length}', should be at least '{count}'.");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator Keyword(int index, string keyword = null)
    {
        Type<KeywordToken>(index);

        if (keyword != null)
        {
            Value(index, keyword);
        }

        return this;
    }

    public TokenValidator StringLiteral(int index, string value = null)
    {
        Type<StringLiteralToken>(index);

        if (value != null)
        {
            Value(index, value);
        }

        return this;
    }

    public TokenValidator Operator(int index, OperatorType operatorType)
    {
        if (!CheckValidTokenIndex(index, $"{operatorType} operator")) return this;

        Type<OperatorToken>(index);
        var token = tokens[index] as OperatorToken;
        if (token?.Type != operatorType)
        {
            Fail(index, $"Invalid operator token {index} value. Expected: '{operatorType}' Actual: '{token?.Type}'");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator MemberAccess(int index, string value = null)
    {
        Type<MemberAccessLiteralToken>(index);
        if (value != null) Value(index, value);
        return this;
    }

    public TokenValidator Comment(int index)
    {
        Type<CommentToken>(index);
        return this;
    }

    public TokenValidator QuotedString(int index, string literal = null)
    {
        var token = ValidateType<QuotedLiteralToken>(index);
        if (token != null && literal != null && token.Value != literal)
        {
            Fail(index, $"Invalid token {index} value. Expected: '{literal}' Actual: '{token.Value}'");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator NumberLiteral(int index, decimal? value = null)
    {
        var token = ValidateType<NumberLiteralToken>(index);
        if (token != null && value != null && token.NumberValue != value)
        {
            Fail(index, $"Invalid token {index} value. Expected: '{value}' Actual: '{token.Value}'");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator Boolean(int index, bool value)
    {
        var token = ValidateType<BooleanLiteral>(index);
        if (token != null && token.BooleanValue != value)
        {
            Fail(index, $"Invalid token {index} value. Expected: '{value}' Actual: '{token.Value}'");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator DateTime(int index, int year, int month, int day, int hours, int minutes, int seconds)
    {
        var token = ValidateType<DateTimeLiteral>(index);
        var value = new DateTime(year, month, day, hours, minutes, seconds);
        if (token != null && token.DateTimeValue != value)
        {
            Fail(index, $"Invalid token value at {index}. Expected: '{value}' Actual: '{token.Value}'");
            IsValid = false;
        }

        return this;
    }

    public TokenValidator Type<T>(int index) where T : Token
    {
        ValidateType<T>(index);
        return this;
    }

    public TokenValidator IsLiteralToken(int index)
    {
        if (!CheckValidTokenIndex(index, "LiterToken")) return this;

        var token = tokens[index];
        if (token is not ILiteralToken literalToken)
        {
            Fail(index, $"Invalid token type as {index}. Expected: 'LiteralToken' Actual: '{token?.GetType().Name})'");
            IsValid = false;

            return this;
        }

        return this;
    }

    private T ValidateType<T>(int index) where T : Token
    {
        if (!CheckValidTokenIndex(index, typeof(T).Name)) return null;

        var token = tokens[index];
        var type = token.GetType();
        if (type != typeof(T))
        {
            Fail(index, $"Invalid token. Expected: '{typeof(T).Name}' Actual: '{type.Name}({token.Value})'");
            IsValid = false;

            return null;
        }

        return (T)token;
    }

    public TokenValidator Value(int index, string expectedValue)
    {
        if (!CheckValidTokenIndex(index, string.Empty)) return this;

        var token = tokens[index];
        if (token.Value != expectedValue)
        {
            Fail(index, $"Invalid token value as {index}. Expected: '{expectedValue}' Actual: '{token.Value}'");
            IsValid = false;
        }

        return this;
    }

    private bool CheckValidTokenIndex(int index, string name)
    {
        if (index < tokens.Length) return true;

        Fail(line.LineEndReference(),  $"Token expected: '{name}'");
        IsValid = false;

        return false;
    }

    private void Fail(SourceReference reference, string error)
    {
        logger.Fail(reference, $"({parserName}) {error}");
    }

    private void Fail(int index, string error)
    {
        logger.Fail(line.TokenReference(index), $"({parserName}) {error}");
    }

    public void Assert()
    {
        if (!IsValid)
        {
            throw new InvalidOperationException(logger.FormatMessages());
        }
    }
}