using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.Parser.Tokens;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Transcribe
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
            var resultType = $"{typeof(FunctionResult).Namespace}.{nameof(FunctionResult)}";

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
                if (variable.Default != null) stringWriter.Write($" = {FormatLiteralValue(variable.Default)}");
                stringWriter.Write(";");
                stringWriter.EndLine();
            }
        }

        private void WriteRunMethod(Function function, ClassWriter writer)
        {
            writer.OpenScope("public void __Run(IExecutionContext executionContext)");

            foreach (var line in function.Code.Lines)
            {
                writer.WriteLine($@"executionContext.LogDebug(@""{Escape(line.SourceLine)}"");");
                writer.WriteLineStart();
                foreach (var token in line.Tokens)
                {
                    writer.Write(FormatLiteralValue(token));
                }
                writer.WriteLineEnd(";");
            }

            writer.CloseScope();
        }

        private string Escape(Line line)
        {
            return line.ToString()
                .Replace(@"""", @"""""");
        }

        private string FormatLiteralValue(Token token)
        {
            return token switch
            {
                //StringLiteralToken _ => $@"""{token.Value}""",
                QuotedLiteralToken _ => $@"""{token.Value}""",
                NumberLiteralToken _ => $@"{token.Value}m",
                _ => token.Value
            };
        }
    }
}