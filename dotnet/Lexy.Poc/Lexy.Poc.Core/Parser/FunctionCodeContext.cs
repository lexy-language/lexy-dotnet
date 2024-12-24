using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class FunctionCodeContext : IFunctionCodeContext
    {
        private readonly Nodes nodes;
        private readonly IParserLogger logger;
        private readonly IDictionary<string, VariableType> variables = new Dictionary<string, VariableType>();

        public FunctionCodeContext(Nodes nodes, IParserLogger logger)
        {
            this.nodes = nodes ?? throw new ArgumentNullException(nameof(nodes));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type)
        {
            if (variables.ContainsKey(name))
            {
                logger.Fail(reference, $"Duplicated variable name: '{name}'");
                return;
            }

            variables.Add(name, type);
        }

        public void EnsureVariableExists(SourceReference reference, string name)
        {
            if (!variables.ContainsKey(name))
            {
                logger.Fail(reference, $"Unknown variable name: '{name}'");
            }
        }

        public VariableType GetVariableType(string name)
        {
            return variables.TryGetValue(name, out var value) ? value : null;
        }
    }
}