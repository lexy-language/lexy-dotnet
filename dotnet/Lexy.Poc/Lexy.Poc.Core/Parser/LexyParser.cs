using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class LexyParser
    {
        public TypeSystem ParseFile(string fileName)
        {
            var code = File.ReadAllLines(fileName);
            var lines = code.Select((line, index) => new Line(index, line, code));

            return Parse(lines);
        }

        public TypeSystem Parse(IEnumerable<Line> lines)
        {
            if (lines == null) throw new ArgumentNullException(nameof(lines));

            var tokens = new List<IToken>();
            var currentIndent = 0;

            Stack<IToken> tokenStack = new Stack<IToken>();
            IRootToken root = null;
            IToken current = null;
            var failed = false;

            foreach (var line in lines)
            {
                var indent = line.Indent();
                if (indent == 0 && !line.IsComment() && !line.IsEmpty())
                {
                    root = GetToken(line);
                    current = root;
                    tokens.Add(current);
                    failed = false;
                    tokenStack.Clear();
                    currentIndent = indent;
                }
                else if (!failed)
                {
                    if (line.IsComment() || line.IsEmpty())
                    {
                        current?.Parse(line);
                    }
                    else
                    {
                        if (current == null)
                        {
                            throw new InvalidOperationException($"Invalid syntax: {line};");
                        }

                        try
                        {
                            if (current is RootToken && indent < currentIndent
                             || !(current is RootToken) && indent <= currentIndent)
                            {
                                current = tokenStack.Pop();
                            }

                            var newToken = current.Parse(line);
                            if (newToken != current)
                            {
                                tokenStack.Push(current);
                                current = newToken;
                            }

                            currentIndent = indent;
                        }
                        catch (InvalidOperationException exception)
                        {
                            Console.Write("Parse token failed: " + exception);

                            tokenStack.Clear();
                            root.Fail(exception);
                            failed = true;
                        }
                    }
                }
            }

            return new TypeSystem(tokens);
        }

        private IRootToken GetToken(Line line)
        {
            var tokenName = TokenName.Parse(line);

            switch (tokenName.Name)
            {
                case TokenNames.Function:
                    return Function.Parse(tokenName);
                case TokenNames.Enum:
                    return EnumDefinition.Parse(tokenName);
                case TokenNames.Scenario:
                    return Scenario.Parse(tokenName);
                case TokenNames.Table:
                    return Table.Parse(tokenName);
            }
            throw new InvalidOperationException($"Unknown keyword: {tokenName.Name}");
        }
    }

    internal class TokenNames
    {
        public const string Function = "Function";
        public const string Enum = "Enum";
        public const string Table = "Table";
        public const string Scenario = "Scenario";

        public const string Include = "Include";
        public const string Parameters = "Parameters";
        public const string Result = "Result";
        public const string Code = "Code";
        public const string ExpectError = "ExpectError";

        public const string Comment = "#";
        public const char CommentChar = '#';
        public const char TableSeparator = '|';
    }
}