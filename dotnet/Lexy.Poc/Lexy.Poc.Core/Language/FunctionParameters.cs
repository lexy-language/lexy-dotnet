using System.Collections.Generic;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionParameters : IToken
    {
        public IList<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        public IToken Parse(Line line)
        {
            if (line.IsEmpty()) return this;

            var variableDefinition = VariableDefinition.Parse(line);
            Variables.Add(variableDefinition);
            return this;
        }
    }
}