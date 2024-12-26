using System;
using System.Collections.Generic;
using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Compiler
{
    public class CompilerResult
    {
        private readonly IDictionary<string, ExecutableFunction> executables;
        private readonly IDictionary<string, Type> enums;

        public CompilerResult(IDictionary<string, ExecutableFunction> executables, IDictionary<string, Type> enums)
        {
            this.executables = executables;
            this.enums = enums;
        }

        public ExecutableFunction GetFunction(Function function)
        {
            return executables[function.NodeName];
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