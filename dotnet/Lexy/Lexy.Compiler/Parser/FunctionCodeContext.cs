using System;
using System.Collections.Generic;
using Lexy.Compiler.Language.Types;

namespace Lexy.Compiler.Parser
{
    public class FunctionCodeContext : IFunctionCodeContext
    {
        private readonly IParserLogger logger;
        private readonly IFunctionCodeContext parentContext;
        private readonly IDictionary<string, VariableEntry> variables = new Dictionary<string, VariableEntry>();

        public FunctionCodeContext(IParserLogger logger, IFunctionCodeContext parentContext)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.parentContext = parentContext;
        }

        public void AddVariable(string name, VariableType type, VariableSource source)
        {
            if (Contains(name)) return;

            var entry = new VariableEntry(type, source);
            variables.Add(name, entry);
        }

        public void RegisterVariableAndVerifyUnique(SourceReference reference, string name, VariableType type, VariableSource source)
        {
            if (Contains(name))
            {
                logger.Fail(reference, $"Duplicated variable name: '{name}'");
                return;
            }

            var entry = new VariableEntry(type, source);
            variables.Add(name, entry);
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
                ? value.VariableType
                : parentContext?.GetVariableType(name);
        }


        public VariableSource? GetVariableSource(string name)
        {
            return variables.TryGetValue(name, out var value)
                ? value.VariableSource
                : parentContext?.GetVariableSource(name);
        }

        public VariableEntry GetVariable(string name)
        {
            return variables.TryGetValue(name, out var value)
                ? value
                : null;
        }
    }
}