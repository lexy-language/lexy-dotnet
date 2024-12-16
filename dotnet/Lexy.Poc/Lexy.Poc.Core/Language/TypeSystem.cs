using System.Collections.Generic;
using System.Linq;

namespace Lexy.Poc.Core.Language
{
    public class TypeSystem
    {
        public IList<IToken> Tokens { get; }

        public int Count => Tokens.Count;

        public TypeSystem(IList<IToken> tokens)
        {
            Tokens = tokens;
        }

        public bool ContainsEnum(string enumName)
        {
            return Tokens
                .OfType<EnumDefinition>()
                .Any(definition => definition.Name.Value == enumName);
        }

        public Function GetFunction(string name)
        {
            return Tokens
                .OfType<Function>()
                .FirstOrDefault(function => function.Name.Value == name);
        }

        public Function GetSingleFunction()
        {
            return Tokens
                .OfType<Function>()
                .SingleOrDefault();
        }

        public IEnumerable<Scenario> GetScenarios() => Tokens.OfType<Scenario>();

        public IRootToken GetEnum(string name)
        {
            return Tokens
                .OfType<EnumDefinition>()
                .FirstOrDefault(enumDefinition => enumDefinition.Name.Value == name);
        }
    }
}