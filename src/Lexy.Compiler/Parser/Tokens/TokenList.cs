using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Compiler.Language;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser.Tokens;

public class TokenList : IEnumerable<Token>
{
    private readonly Token[] values;

    public Token this[int index] => values[index];

    public int Length => values.Length;
    public Line Line { get; }

    public TokenList(Line line, params Token[] values)
    {
        Line = Assert.NotNull(line, nameof(line));
        this.values = Assert.NotNull(values, nameof(values));
    }

    public IEnumerator<Token> GetEnumerator()
    {
        return ((IEnumerable<Token>)values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return values.GetEnumerator();
    }

    public bool IsComment()
    {
        return values.Length == 1 && values[0] is CommentToken;
    }

    public Token TokenAt(int column)
    {
        var columnIndex = column - 1;
        for (var index = 0; index < values.Length; index++)
        {
            var token = values[index];
            if (index == values.Length - 1)
            {
                if (columnIndex >= token.FirstCharacter.Position && columnIndex <= token.EndColumn + 1)
                {
                    return token;
                }
            } else if (token.FirstCharacter.Position >= columnIndex && token.EndColumn + 1 <= columnIndex)
            {
                return token;
            }
        }

        return null;
    }

    public string TokenValue(int index)
    {
        return index >= 0 && index <= values.Length - 1 ? values[index].Value : null;
    }

    public TokenList TokensFrom(int index)
    {
        if (index == values.Length) return new TokenList(Line, Array.Empty<Token>());
        CheckValidTokenIndex(index);

        return new TokenList(Line, values[index..]);
    }

    public TokenList TokensFromStart(int count)
    {
        return new TokenList(Line, values[..count]);
    }

    public TokenList TokensRange(int start, int last)
    {
        var length = last + 1 - start;
        var range = new Token[length];

        Array.Copy(values, start, range, 0, length);

        return new TokenList(Line, range);
    }

    public bool IsTokenType<T>(int index) where T : Token
    {
        return index >= 0 && index <= values.Length - 1 && values[index].GetType() == typeof(T);
    }

    public T Token<T>(int index) where T : Token
    {
        CheckValidTokenIndex(index);

        return (T)values[index];
    }

    public ILiteralToken LiteralToken(int index)
    {
        CheckValidTokenIndex(index);

        return index >= 0 && index <= values.Length - 1 ? values[index] as ILiteralToken : null;
    }

    public bool IsLiteralToken(int index)
    {
        return index >= 0 && index <= values.Length - 1 && values[index] is ILiteralToken;
    }

    public bool IsQuotedString(int index)
    {
        return index >= 0 && index <= values.Length - 1 && values[index] is QuotedLiteralToken;
    }

    public bool IsKeyword(int index, string keyword)
    {
        return index >= 0
            && index <= values.Length - 1
            && (values[index] as KeywordToken)?.Value == keyword;
    }

    public bool IsOperatorToken(int index, OperatorType type)
    {
        return index >= 0
            && index <= values.Length - 1
            && values[index] is OperatorToken operatorToken
            && operatorToken.Type == type;
    }

    public OperatorToken OperatorToken(int index)
    {
        return index >= 0
            && index <= values.Length - 1
            && values[index] is OperatorToken operatorToken
            ? operatorToken
            : null;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        foreach (var value in values)
        {
            builder.Append($"{value.GetType().Name}('{value.Value}') ");
        }
        return builder.ToString();
    }

    private void CheckValidTokenIndex(int index)
    {
        if (index < 0 || index >= values.Length)
        {
            throw new InvalidOperationException($"Invalid token index {index} (length: {values.Length})");
        }
    }

    public int? CharacterColumn(int tokenIndex)
    {
        if (tokenIndex < 0 || tokenIndex >= values.Length) return null;

        return values[tokenIndex].FirstCharacter.Position;
    }

    public int? LastColumn()
    {
        return values[^1].EndColumn;
    }

    public int Find<T>(Func<T, bool> func) where T : Token
    {
        Assert.NotNull(func, nameof(func));

        for (var index = 0; index < values.Length; index++)
        {
            var value = values[index];
            if (value is T specificToken && func(specificToken))
            {
                return index;
            }
        }

        return -1;
    }

    public SourceReference Reference(int tokenIndex, int? numberOfTokens = null)
    {
        Assert.True(numberOfTokens is null or >= 1, $"numberOfTokens should be >= 1 ({numberOfTokens})");

        var column = CharacterColumn(tokenIndex) + 1;
        if (column == null)
        {
            throw new InvalidOperationException("TokenReference: " + tokenIndex);
        }

        var endColumn = numberOfTokens != null
            ? CharacterColumn(tokenIndex + numberOfTokens.Value)
            : LastColumn() ;
        if (column == null)
        {
            throw new InvalidOperationException($"TokenReference end: {tokenIndex + numberOfTokens}");
        }
        endColumn = endColumn == null ? Line.Content.Length : endColumn + 1;

        return new SourceReference(Line.FileName, Line.Index + 1, column.Value, endColumn.Value);
    }

    public SourceReference AllReference()
    {
        if (Length == 0)
        {
            return new SourceReference(Line.FileName ?? "runtime", Line.Index + 1, 1, Line.Content.Length + 1);
        }

        var column = this[0].FirstCharacter.Position + 1;
        var columnEnd = this[^1].EndColumn + 1;
        return new SourceReference(Line.FileName ?? "runtime", Line.Index + 1, column, columnEnd);
    }
}
