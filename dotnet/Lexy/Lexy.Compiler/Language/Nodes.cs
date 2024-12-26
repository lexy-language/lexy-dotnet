using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class Nodes : IEnumerable<INode>
    {
        private readonly IList<IRootNode> values = new Collection<IRootNode>();

        public int Count => values.Count;

        internal bool ContainsEnum(string enumName)
        {
            return values
                .OfType<EnumDefinition>()
                .Any(definition => definition.Name.Value == enumName);
        }

        public IRootNode GetNode(string name)
        {
            return values
                .FirstOrDefault(definition => definition.NodeName == name);
        }

        public bool Contains(string name)
        {
            return values
                .Any(definition => definition.NodeName == name);
        }

        public Function GetFunction(string name)
        {
            return values
                .OfType<Function>()
                .FirstOrDefault(function => function.Name.Value == name);
        }

        public Table GetTable(string name)
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

        public EnumDefinition GetEnum(string name)
        {
            return values
                .OfType<EnumDefinition>()
                .FirstOrDefault(enumDefinition => enumDefinition.Name.Value == name);
        }

        public void AddIfNew(IRootNode node)
        {
            if (!values.Contains(node))
            {
                values.Add(node);
            }
        }

        public INode First() => values.FirstOrDefault();

        public VariableType GetType(string name)
        {
            var node = GetNode(name);
            return node switch
            {
                Table table => new TableType(name, table),
                EnumDefinition enumDefinition => new EnumType(name, enumDefinition),
                _ => null
            };
        }

        public IEnumerator<INode> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}