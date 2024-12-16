using System;

namespace Lexy.Poc.Core.Language
{
    public interface IRootToken : IToken
    {
        string TokenName { get; }
        void Fail(Exception exception);
    }
}