using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Poc.Core.Compiler.CSharp;
using Lexy.Poc.Core.RunTime;

namespace Lexy.Poc.Core.Compiler
{
    public class ExecutableFunction
    {
        private readonly object[] emptyParameters = Array.Empty<object>();

        private readonly object functionObject;
        private readonly IExecutionContext context;
        private readonly MethodInfo resultMethod;

        private readonly MethodInfo runMethod;
        private readonly IDictionary<string, FieldInfo> variables = new Dictionary<string, FieldInfo>();

        public ExecutableFunction(object functionObject, IExecutionContext context)
        {
            this.functionObject = functionObject ?? throw new ArgumentNullException(nameof(functionObject));
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            runMethod = functionObject.GetType().GetMethod( LexyCodeConstants.RunMethod, BindingFlags.Instance | BindingFlags.Public);
            resultMethod = functionObject.GetType().GetMethod(LexyCodeConstants.ResultMethod, BindingFlags.Instance | BindingFlags.Public);
        }

        public FunctionResult Run()
        {
            runMethod.Invoke(functionObject, new [] { context });

            return (FunctionResult)resultMethod.Invoke(functionObject, emptyParameters);
        }

        public FunctionResult Run(IDictionary<string, object> values)
        {
            foreach (var value in values)
            {
                var field = GetParameterField(value.Key);
                var convertedValue = Convert.ChangeType(value.Value, field.FieldType);
                field.SetValue(functionObject, convertedValue);
            }

            runMethod.Invoke(functionObject, new []{context});

            return (FunctionResult)resultMethod.Invoke(functionObject, emptyParameters);
        }

        private FieldInfo GetParameterField(string name)
        {
            if (variables.ContainsKey(name)) return variables[name];

            var field = functionObject.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field == null) throw new InvalidOperationException("Couldn't find parameter field: " + name);

            variables[name] = field;
            return field;
        }
    }
}