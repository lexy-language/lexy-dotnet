using System;

namespace Lexy.Poc.Core.Parser
{
    public class Line
    {
        public int Index { get; }

        internal string Content { get; }
        private string TrimmedContent { get; }

        public TokenList Tokens { get; private set; }

        public Line(int index, string line)
        {
            Index = index;
            Content = line ?? throw new ArgumentNullException(nameof(line));
            TrimmedContent = line.Trim();
        }

        public int? Indent(IParserContext parserContext)
        {
            var spaces = 0;
            var tabs = 0;

            var index = 0;
            for (; index < Content.Length; index++)
            {
                var value = Content[index];
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
                parserContext.Logger.Fail(parserContext.LineReference(index), "Don't mix spaces and tabs for indentations. Use 2 spaces or tabs.");
                return null;
            }

            if (spaces % 2 != 0)
            {
                parserContext.Logger.Fail(parserContext.LineReference(index), $"Wrong number of indent spaces {spaces}. Should be multiplication of 2. (line: {Index} line: {Content})");
                return null;
            }

            return tabs > 0 ? tabs : spaces / 2;
        }

        public bool Tokenize(ITokenizer tokenizer, IParserContext parserContext)
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

        public int? FirstCharacter()
        {
            for (var index = 0; index < Content.Length; index++)
            {
                if (Content[index] != ' ' && Content[index] != '\\')
                {
                    return index;
                }
            }

            return null;
        }
    }
}