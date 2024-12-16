using System;
using System.Collections.Generic;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class FunctionResult : IToken
    {
        public IList<VariableDefinition> Variables { get; } = new List<VariableDefinition>();

        public IToken Parse(Line line)
        {
            var variableDefinition = VariableDefinition.Parse(line);
            if (variableDefinition.Default != null)
                throw new InvalidOperationException("Result variable " + variableDefinition.Name +
                                                    " should not have a default value. (Line:" + line + ")");
            Variables.Add(variableDefinition);

            return this;
        }

        public string GetParameterType(string expectedName)
        {
            return Variables.FirstOrDefault(variable => variable.Name == expectedName)?.Type;
        }
    }
}