using System;
using System.Linq;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Shouldly;

namespace Lexy.Poc
{
    public static class ParserExtensions
    {
        public static Function ParseFunction(this LexyParser parser, string code)
        {
            var codeLines = code.Split(Environment.NewLine);
            var lines = codeLines
                .Select((line, index) => new Line(index, line, codeLines));

            var typeSystem = parser.Parse(lines);

            if (typeSystem.Count != 1)
            {
                throw new InvalidOperationException("Only 1 token expected. Actual: " + typeSystem.Count);
            }

            var function = typeSystem.Tokens[0] as Function;
            if (function == null)
            {
                throw new InvalidOperationException("Token not a function. Actual: " + typeSystem.Tokens[0].GetType());
            }
            function.FailedException.ShouldBeNull();

            return function;
        }

        public static TypeSystem ParseFunctionCode(this LexyParser parser, string code)
        {
            var codeLines = code.Split(Environment.NewLine);
            var lines = codeLines
                .Select((line, index) => new Line(index, line, codeLines));

            var typeSystem = parser.Parse(lines);

            if (typeSystem.Count != 1)
            {
                throw new InvalidOperationException("Only 1 token expected. Actual: " + typeSystem.Count);
            }

            var function = typeSystem.Tokens[0] as Function;
            if (function == null)
            {
                throw new InvalidOperationException("Token not a function. Actual: " + typeSystem.Tokens[0].GetType());
            }
            function.FailedException.ShouldBeNull();

            return typeSystem;
        }
    }
}