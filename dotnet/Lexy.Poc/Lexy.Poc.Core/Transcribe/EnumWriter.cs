using System;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class EnumWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(ClassWriter writer, IRootComponent component, Components components)
        {
            if (!(component is EnumDefinition enumDefinition))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var name = enumDefinition.Name.Value;

            writer.OpenScope($"public enum {name}");

            WriteValues(enumDefinition, writer, components);

            writer.CloseScope();

            return new GeneratedClass(enumDefinition, name);

        }

        private void WriteValues(EnumDefinition enumDefinition, ClassWriter classWriter, Components components)
        {
            foreach (var value in enumDefinition.Assignments)
            {
                classWriter.WriteLine($"{value.Name} = {value.Value},");
            }
        }
    }
}