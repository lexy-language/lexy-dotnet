using System;

namespace Lexy.Compiler.Language;

public abstract class ParseResult<T>
{
    private readonly T result;

    public string ErrorMessage { get; }
    public bool IsSuccess { get; }

    public T Result
    {
        get
        {
            if (!IsSuccess) throw new InvalidOperationException("ParseResult not successful.");
            return result;
        }
    }

    protected ParseResult(T result)
    {
        this.result = result;
        IsSuccess = true;
    }

    protected ParseResult(bool success, string errorMessage)
    {
        ErrorMessage = errorMessage;
        IsSuccess = success;
    }
}