using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Compiler
{
    public class ExecutionEnvironment
    {
        private readonly IList<GeneratedClass> generatedTypes = new List<GeneratedClass>();
        private readonly IDictionary<string, ExecutableFunction> executables = new Dictionary<string, ExecutableFunction>();
        private readonly IDictionary<string, Type> enums = new Dictionary<string, Type>();
        private readonly IDictionary<string, Type> tables = new Dictionary<string, Type>();

        public void CreateExecutables(Assembly assembly)
        {
            foreach (var generatedClass in generatedTypes)
            {
                if (generatedClass.Component is Function)
                {
                    var instance = assembly.CreateInstance(generatedClass.FullClassName);
                    var executable = new ExecutableFunction(instance);

                    executables.Add(generatedClass.Component.ComponentName, executable);
                }
                else if (generatedClass.Component is EnumDefinition)
                {
                    var enumType = assembly.GetType(generatedClass.FullClassName);
                    enums.Add(generatedClass.Component.ComponentName, enumType);
                }
                else if (generatedClass.Component is Table)
                {
                    var table = assembly.GetType(generatedClass.FullClassName);
                    tables.Add(generatedClass.Component.ComponentName, table);
                }
                else
                {
                    throw new InvalidOperationException("Unknown generated type: " + generatedClass.Component.GetType());
                }
            }
        }

        internal void AddType(GeneratedClass generatedType)
        {
            generatedTypes.Add(generatedType);
        }

        public ExecutableFunction GetFunction(Function function)
        {
            return executables[function.ComponentName];
        }

        public bool ContainsEnum(string type)
        {
            return enums.ContainsKey(type);
        }

        public Type GetEnumType(string type)
        {
            return enums[type];
        }
    }
}