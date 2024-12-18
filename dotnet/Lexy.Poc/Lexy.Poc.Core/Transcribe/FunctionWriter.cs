using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class FunctionWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(ClassWriter writer, IRootComponent component, Components components)
        {
            if (!(component is Function function))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var name = function.Name.ClassName();

            writer.OpenScope($"public class {name}");

            WriteParameters(function, writer, components);
            WriteIncludes(function, writer, components);
            WriteResult(function, writer, components);
            WriteRunMethod(function, writer);

            writer.CloseScope();

            return new GeneratedClass(function, name);
        }

        private void WriteParameters(Function function, ClassWriter stringWriter, Components components)
        {
            WriteVariables(stringWriter, function.Parameters.Variables, components);
        }

        private void WriteIncludes(Function function, ClassWriter stringWriter, Components components)
        {
            foreach (var include in function.Include.Definitions)
            {
                if (include.Type != IncludeTypes.Table)
                    throw new InvalidOperationException("Invalid include type: " + include.Type);

                stringWriter.WriteLine($"public {include.Name} {include.Name} = new {include.Name}();");
            }
        }

        private void WriteResult(Function function, ClassWriter stringWriter, Components components)
        {
            var variables = function.Results.Variables;

            WriteVariables(stringWriter, variables, components);
            WriteResultMethod(stringWriter, variables);
        }

        private void WriteResultMethod(ClassWriter stringWriter, IList<VariableDefinition> resultVariables)
        {
            var resultType = $"{typeof(Core.FunctionResult).Namespace}.{nameof(Core.FunctionResult)}";

            stringWriter.OpenScope($"public {resultType} __Result()");
            stringWriter.WriteLine($"var result = new {resultType}();");

            foreach (var variable in resultVariables)
                stringWriter.WriteLine($"result[\"{variable.Name}\"] = {variable.Name};");

            stringWriter.WriteLine("return result;");
            stringWriter.CloseScope();
        }

        private void WriteVariables(ClassWriter stringWriter, IList<VariableDefinition> variables, Components components)
        {
            foreach (var variable in variables)
            {
                stringWriter.WriteLineStart($"public {components.MapType(variable.Type)} {variable.Name}");
                if (variable.Default != null) stringWriter.Write($" = {variable.Default.Value}");
                stringWriter.Write(";");
                stringWriter.EndLine();
            }
        }

        private void WriteRunMethod(Function function, ClassWriter writer)
        {
            writer.OpenScope("public void __Run()");

            foreach (var line in function.Code.Lines)
            {
                if (!string.IsNullOrWhiteSpace(line.Value))
                {
                    writer.WriteLine($"{line.Value};");
                }
            }

            writer.CloseScope();
        }
    }
}