using System;
using System.Linq;

namespace Lexy.Poc.Core.Parser
{
    public class SourceCodeDocument : ISourceCodeDocument
    {
        private int index;

        private Line[] code;

        public SourceFile File { get; private set; }

        public Line CurrentLine { get; private set; }

        public void SetCode(string[] lines, string fileName)
        {
            if (code != null)
            {
                throw new InvalidOperationException(
                    "Source code already set. Each LexyParser should only be used once. " +
                    "Scope should be managed by using ServiceProvider.OpenScope(). " +
                    "See SpecificationFileRunner.Create as example.");
            }

            index = -1;
            code = lines.Select((line, index) => new Line(index, line)).ToArray();
            File = new SourceFile(fileName);
        }

        public bool HasMoreLines()
        {
            return index < code.Length - 1;
        }

        public Line NextLine()
        {
            if (index >= code.Length)
            {
                throw new InvalidOperationException("No more lines");
            }

            CurrentLine = code[++index];
            return CurrentLine;
        }

        public void Reset()
        {
            CurrentLine = null;
        }
    }
}