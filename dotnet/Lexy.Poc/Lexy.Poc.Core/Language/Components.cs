using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Lexy.Poc.Core.Language
{
    public class Components
    {
        private readonly IList<IRootComponent> values = new Collection<IRootComponent>();

        public int Count => values.Count;

        private bool ContainsEnum(string enumName)
        {
            return values
                .OfType<EnumDefinition>()
                .Any(definition => definition.Name.Value == enumName);
        }

        public IRootComponent GetComponent(string name)
        {
            return values
                .SingleOrDefault(definition => definition.ComponentName == name);
        }

        public bool Contains(string name)
        {
            return values
                .Any(definition => definition.ComponentName == name);
        }

        public Function GetFunction(string name)
        {
            return values
                .OfType<Function>()
                .FirstOrDefault(function => function.Name.Value == name);
        }

        public IRootComponent GetTable(string name)
        {
            return values
                .OfType<Table>()
                .FirstOrDefault(function => function.Name.Value == name);
        }

        public Function GetSingleFunction()
        {
            return values
                .OfType<Function>()
                .SingleOrDefault();
        }

        public IEnumerable<Scenario> GetScenarios() => values.OfType<Scenario>();

        public IRootComponent GetEnum(string name)
        {
            return values
                .OfType<EnumDefinition>()
                .FirstOrDefault(enumDefinition => enumDefinition.Name.Value == name);
        }

        public void AddIfNew(IRootComponent component)
        {
            if (!values.Contains(component))
            {
                values.Add(component);
            }
        }

        public IComponent First() => values.FirstOrDefault();

        public string MapType(string variableType)
        {
            if (ContainsEnum(variableType))
            {
                return variableType;
            }

            return variableType switch
            {
                TypeNames.Number => "decimal",
                TypeNames.Boolean => "bool",
                TypeNames.DateTime => "System.DateTime",
                _ => throw new InvalidOperationException("Unknown type: " + variableType)
            };
        }
    }
}