using System;

namespace Lexy.Poc.Core.Parser
{
    public class Line
    {
        public int Index { get; }

        internal string Content { get; }
        private string TrimmedContent { get; }

        internal TokenList Tokens { get; private set; }

        public Line(int index, string line)
        {
            Index = index;
            Content = line ?? throw new ArgumentNullException(nameof(line));
            TrimmedContent = line.Trim();
        }

        public int Indent()
        {
            var spaces = 0;
            var tabs = 0;

            foreach (var value in Content)
            {
                if (value == ' ')
                {
                    spaces++;
                }
                else if (value == '\t')
                {
                    tabs++;
                }
                else
                {
                    break;
                }
            }

            if (spaces > 0 && tabs > 0)
            {
                throw new InvalidOperationException(
                    "Don't mix spaces and tabs for indentations. Use 2 spaces or tabs.");
            }

            if (spaces % 2 != 0)
            {
                throw new InvalidOperationException(
                    "Wrong number of indent spaces " + spaces + ". Should be multiplication of 2.");
            }

            return tabs > 0 ? tabs : spaces / 2;
        }

        internal bool Tokenize(ITokenizer tokenizer, ParserContext parserContext)
        {
            Tokens = tokenizer.Tokenize(this, parserContext, out bool errors);
            return !errors;
        }

        public override string ToString()
        {
            return $"{Index + 1}: {Content}";
        }

        public bool IsEmpty() => Content.Length == 0;

        public bool IsComment() => Tokens.IsComment();
    }
}