using System;
using System.IO;

namespace Lexy.Poc.Core.Parser
{
    public class ClassWriter
    {
        private const int intendSize = 4;

        private readonly StringWriter stringWriter = new StringWriter();
        private int intend;

        public void WriteLine(string text)
        {
            WriteIntend();
            stringWriter.WriteLine(text);
        }

        private void WriteIntend()
        {
            if (intend > 0)
            {
                stringWriter.Write(new string(' ', intend));
            }
        }

        public void OpenScope(string text)
        {
            WriteLine(text);
            WriteLine("{");

            intend += intendSize;
        }

        public void CloseScope()
        {
            if (intend == 0)
            {
                throw new InvalidOperationException("Intend cannot be smaller as 0");
            }

            intend -= intendSize;
            WriteLine("}");
        }

        public void WriteLineStart(string text)
        {
            WriteIntend();
            Write(text);
        }

        public void Write(string text)
        {
            stringWriter.Write(text);
        }

        public override string ToString()
        {
            return stringWriter.ToString();
        }

        public void EndLine()
        {
            stringWriter.WriteLine();
        }
    }
}