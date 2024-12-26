using System;
using System.Collections.Generic;
using System.Reflection;
using Lexy.Compiler.Compiler.CSharp;
using Lexy.RunTime.RunTime;

namespace Lexy.Compiler.Compiler
{
    public class ExecutableFunction
    {
        private readonly object[] emptyParameters = Array.Empty<object>();

        private readonly Type functionObject;
        private readonly IExecutionContext context;

        private readonly MethodInfo runMethod;
        private readonly IDictionary<string, FieldInfo> variables = new Dictionary<string, FieldInfo>();
        private readonly Type parametersType;

        public ExecutableFunction(Type functionObject, IExecutionContext context)
        {
            this.functionObject = functionObject ?? throw new ArgumentNullException(nameof(functionObject));
            this.context = context ?? throw new ArgumentNullException(nameof(context));

            runMethod = functionObject.GetMethod( LexyCodeConstants.RunMethod, BindingFlags.Static | BindingFlags.Public);
            parametersType = functionObject.GetNestedType(LexyCodeConstants.ParameterType);
        }

        public FunctionResult Run() => Run(new Dictionary<string, object>());

        public FunctionResult Run(IDictionary<string, object> values)
        {
            var parameters = CreateParameters();

            foreach (var value in values)
            {
                var field = GetParameterField(parameters, value.Key);
                var convertedValue = Convert.ChangeType(value.Value, field.FieldType);
                field.SetValue(parameters, convertedValue);
            }

            var results = runMethod.Invoke(null, new []{parameters, context});

            return new FunctionResult(results);
        }

        private object CreateParameters() => Activator.CreateInstance(parametersType);

        private FieldInfo GetParameterField(object parameters, string name)
        {
            if (variables.ContainsKey(name)) return variables[name];

            var type = parameters.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field == null) throw new InvalidOperationException($"Couldn't find parameter field: '{name}' on type: '{type.Name}'");

            variables[name] = field;
            return field;
        }
    }
}