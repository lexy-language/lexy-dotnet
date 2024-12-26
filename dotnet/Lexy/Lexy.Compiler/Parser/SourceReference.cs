using System;

namespace Lexy.Poc.Core.Parser
{
    public class SourceReference
    {
        private readonly int? lineNumber;
        private readonly int? characterNumber;

        public SourceFile File { get; }

        public SourceReference(SourceFile file, int? lineNumber, int? characterNumber)
        {
            File = file ?? throw new ArgumentNullException(nameof(file));
            this.characterNumber = characterNumber;
            this.lineNumber = lineNumber;
        }

        public override string ToString() => $"{File.FileName}({lineNumber}, {characterNumber})";
    }
}