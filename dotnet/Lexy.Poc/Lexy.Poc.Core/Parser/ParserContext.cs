using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class ParserContext
    {
        private readonly IList<string> messages = new List<string>();
        private readonly ITokenizer tokenizer = new Tokenizer();

        private int failedMessages = 0;
        private IRootComponent currentComponent;

        internal Line CurrentLine { get; private set; }

        public Components Components { get; } = new Components();

        public Line[] Code { get; }

        public bool HasErrors() => failedMessages > 0;

        public ParserContext(Line[] code)
        {
            Code = code;
        }

        public void Log(string message)
        {
            messages.Add($"{CurrentLine?.Index + 1}: {message}");
        }

        public void Fail(string message)
        {
            failedMessages++;
            messages.Add($"{CurrentLine?.Index + 1}: ERROR - {message}");
            currentComponent?.Fail(message);
        }

        public void ProcessComponent(IRootComponent component)
        {
            if (component == null) throw new ArgumentNullException(nameof(component));

            Components.Add(component);
            currentComponent = component;
        }

        public bool ProcessLine(Line line)
        {
            CurrentLine = line ?? throw new ArgumentNullException(nameof(line));
            Log(line.ToString());

            var success = CurrentLine.Tokenize(tokenizer, this);
            var tokenNames = string.Join(" ", CurrentLine.Tokens.Select(token => token.GetType().Name).ToArray());

            Log("  Tokens: " + tokenNames);

            return success;
        }

        public TokenValidator ValidateTokens<T>()
        {
            Log("  Parse: " + typeof(T).Name);
            return new TokenValidator(this);
        }

        public TokenValidator ValidateTokens(string name)
        {
            Log("  Parse: " + name);
            return new TokenValidator(this);
        }

        public bool HasErrorMessage(string expectedError)
        {
            return failedMessages > 0;
        }

        public string FormatMessages()
        {
            return
                $"{string.Join(Environment.NewLine, messages)}{Environment.NewLine}------------- Lexy Source Code{Environment.NewLine}{FormatCode()}";
        }

        private string FormatCode()
        {
            var builder = new StringBuilder();
            foreach (var line in Code)
            {
                builder.AppendLine(line.ToString());
            }
            return builder.ToString();
        }
    }
}