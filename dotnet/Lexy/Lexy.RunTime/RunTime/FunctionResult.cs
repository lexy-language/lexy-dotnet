using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lexy.RunTime.RunTime
{
    public class FunctionResult
    {
        private readonly IDictionary<string, FieldInfo> variables = new Dictionary<string, FieldInfo>();

        private readonly object valueObject;

        public FunctionResult(object valueObject)
        {
            this.valueObject = valueObject;
        }

        public object this[string name] => GetField(name).GetValue(valueObject);

        public decimal Number(string name)
        {
            return (decimal) this[name];
        }

        private FieldInfo GetField(string name)
        {
            if (variables.ContainsKey(name)) return variables[name];

            var type = valueObject.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field == null) throw new InvalidOperationException($"Couldn't find field: '{name}' on type: '{type.Name}'");

            variables[name] = field;
            return field;
        }
    }
}