using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class WriterCode
    {
        public const string Namespace = "Lexy.Runtime";
    }
    public class FunctionWriter : IRootTokenWriter
    {

        public GeneratedClass CreateCode(IRootToken token, TypeSystem typeSystem)
        {
            if (!(token is Function function))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var classWriter = new ClassWriter();

            classWriter.OpenScope($"namespace {WriterCode.Namespace}");

            var name = function.Name.ClassName();

            classWriter.OpenScope($"public class {name}");

            WriteParameters(function, classWriter, typeSystem);
            WriteResult(function, classWriter, typeSystem);
            WriteRunMethod(function, classWriter);

            classWriter.CloseScope();
            classWriter.CloseScope();

            return new GeneratedClass(function, WriterCode.Namespace, name, classWriter.ToString());
        }

        private void WriteParameters(Function function, ClassWriter stringWriter, TypeSystem typeSystem)
        {
            WriteVariables(stringWriter, function.Parameters.Variables, typeSystem);
        }

        private void WriteResult(Function function, ClassWriter stringWriter, TypeSystem typeSystem)
        {
            WriteVariables(stringWriter, function.Result.Variables, typeSystem);
            WriteResultMethod(stringWriter, function.Result.Variables);
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

        private void WriteVariables(ClassWriter stringWriter, IList<VariableDefinition> variables, TypeSystem typeSystem)
        {
            foreach (var variable in variables)
            {
                stringWriter.WriteLineStart($"public {MapType(variable.Type, typeSystem)} {variable.Name}");
                if (variable.Default != null) stringWriter.Write($" = {variable.Default}");
                stringWriter.Write(";");
                stringWriter.EndLine();
            }
        }

        private string MapType(string variableType, TypeSystem typeSystem)
        {
            if (typeSystem.ContainsEnum(variableType))
            {
                return variableType;
            }

            return variableType switch
            {
                TypeNames.Int => "int",
                TypeNames.Number => "decimal",
                TypeNames.Boolean => "bool",
                TypeNames.DateTime => "System.DateTime",
                _ => throw new InvalidOperationException("Unknown type: " + variableType)
            };
        }

        private void WriteRunMethod(Function function, ClassWriter stringWriter)
        {
            stringWriter.OpenScope("public void __Run()");

            foreach (var line in function.Code.Lines)
            {
                if (!string.IsNullOrWhiteSpace( line))
                {
                    stringWriter.WriteLine($"{line};");
                }
            }

            stringWriter.CloseScope();
        }
    }

    public class EnumWriter : IRootTokenWriter
    {
        public GeneratedClass CreateCode(IRootToken token, TypeSystem typeSystem)
        {
            if (!(token is EnumDefinition enumDefinition))
            {
                throw new InvalidOperationException("Root token not Function");
            }

            var classWriter = new ClassWriter();
            var name = enumDefinition.Name.Value;

            classWriter.OpenScope($"namespace {WriterCode.Namespace}");
            classWriter.OpenScope($"public enum {name}");

            WriteValues(enumDefinition, classWriter, typeSystem);

            classWriter.CloseScope();
            classWriter.CloseScope();

            return new GeneratedClass(enumDefinition, WriterCode.Namespace, name, classWriter.ToString());

        }

        private void WriteValues(EnumDefinition enumDefinition, ClassWriter classWriter, TypeSystem typeSystem)
        {
            foreach (var value in enumDefinition.Assignments)
            {
                classWriter.WriteLine($"{value.Name} = {value.Value},");
            }
        }
    }
}