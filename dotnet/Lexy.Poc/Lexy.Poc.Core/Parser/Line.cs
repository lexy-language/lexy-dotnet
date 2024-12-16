using System;
using System.IO;
using System.Linq;

namespace Lexy.Poc.Core.Parser
{
    public class Line
    {
        private readonly string[] code;

        public int Index { get; }
        public string Content { get; }
        public string TrimmedContent { get; }

        public Line(int index, string line, string[] code)
        {
            this.code = code;
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

        public override string ToString()
        {
            var formattedCode = new StringWriter();

            code.Select((line, index) => $"{index + 1}: {line}")
                .ForEach(formattedCode.WriteLine);

            return $"Line {Index + 1}: '{Content}'{Environment.NewLine}Code:{Environment.NewLine}{formattedCode}";
        }

        public bool IsComment() => Content.StartsWith("#");

        public bool IsEmpty() => Content.Length == 0;

        public string FirstTokenName()
        {
            var indexOfSpace = TrimmedContent.IndexOf(" ", StringComparison.Ordinal);
            return indexOfSpace == -1 ? TrimmedContent : TrimmedContent[..indexOfSpace].Trim();
        }

        public string Parameter()
        {
            var indexOfSpace = TrimmedContent.IndexOf(" ", StringComparison.Ordinal);
            return indexOfSpace == -1 ? string.Empty : TrimmedContent[(indexOfSpace + 1)..].Trim();
        }
    }
}