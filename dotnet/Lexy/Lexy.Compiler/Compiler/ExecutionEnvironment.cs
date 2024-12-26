using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Poc.Core.Compiler.CSharp;
using Lexy.Poc.Core.Language;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler
{
    public class ExecutionEnvironment : IExecutionEnvironment
    {
        private readonly IList<GeneratedClass> generatedTypes = new List<GeneratedClass>();
        private readonly IDictionary<string, ExecutableFunction> executables = new Dictionary<string, ExecutableFunction>();
        private readonly IDictionary<string, Type> enums = new Dictionary<string, Type>();
        private readonly IDictionary<string, Type> tables = new Dictionary<string, Type>();

        private readonly IExecutionContext executionContext;

        public ExecutionEnvironment(IExecutionContext executionContext)
        {
            this.executionContext = executionContext ?? throw new ArgumentNullException(nameof(executionContext));
        }

        public void CreateExecutables(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            foreach (var generatedClass in generatedTypes)
            {
                switch (generatedClass.Node)
                {
                    case Function _:
                    {
                        var instance = assembly.CreateInstance(generatedClass.FullClassName);
                        var executable = new ExecutableFunction(instance, executionContext);

                        executables.Add(generatedClass.Node.NodeName, executable);
                        break;
                    }
                    case EnumDefinition _:
                    {
                        var enumType = assembly.GetType(generatedClass.FullClassName);
                        enums.Add(generatedClass.Node.NodeName, enumType);
                        break;
                    }
                    case Table _:
                    {
                        var table = assembly.GetType(generatedClass.FullClassName);
                        tables.Add(generatedClass.Node.NodeName, table);
                        break;
                    }
                    default:
                        throw new InvalidOperationException("Unknown generated type: " + generatedClass.Node.GetType());
                }
            }
        }

        public void AddType(GeneratedClass generatedType)
        {
            generatedTypes.Add(generatedType);
        }

        public CompilerResult Result()
        {
            return new CompilerResult(executables, enums);
        }
    }
}