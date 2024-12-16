using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class RootToken : IRootToken
    {
        public Exception FailedException { get; private set; }

        public abstract string TokenName { get; }

        public abstract IToken Parse(Line line);

        public void Fail(Exception exception)
        {
            FailedException = exception;
        }
    }
}