using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public abstract class RootComponent : IRootComponent
    {
        private readonly IList<string> failedMessages = new List<string>();

        public IEnumerable<string> FailedMessages => failedMessages;
        public bool HasErrors => failedMessages.Count > 0;

        public abstract string ComponentName { get; }

        public abstract IComponent Parse(ParserContext context);

        public void Fail(string message)
        {
            failedMessages.Add(message);
        }
    }
}