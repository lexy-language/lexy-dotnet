using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class FunctionCodeContext : IFunctionCodeContext
    {
        private readonly IParserLogger logger;
        private readonly IFunctionCodeContext parentContext;
        private readonly IDictionary<string, VariableType> variables = new Dictionary<string, VariableType>();

        public FunctionCodeContext(IParserLogger logger, IFunctionCodeContext parentContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.parentContext = parentContext;
        }

        public void AddVariable(string name, VariableType type)
        {
            if (!Contains(name))
            {
                variables.Add(name, type);
            }
        }

        public void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type)
        {
            if (Contains(name))
            {
                logger.Fail(reference, $"Duplicated variable name: '{name}'");
                return;
            }

            variables.Add(name, type);
        }

        public bool Contains(string name)
        {
            return variables.ContainsKey(name) || parentContext != null && parentContext.Contains(name);
        }

        public void EnsureVariableExists(SourceReference reference, string name)
        {
            if (!Contains(name))
            {
                logger.Fail(reference, $"Unknown variable name: '{name}'");
            }
        }

        public VariableType GetVariableType(string name)
        {
            return variables.TryGetValue(name, out var value)
                ? value
                : parentContext?.GetVariableType(name);
        }
    }
}