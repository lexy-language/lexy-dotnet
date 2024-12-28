using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Lexy.RunTime
{
    public class FunctionResult
    {
        private readonly object valueObject;

        public FunctionResult(object valueObject)
        {
            this.valueObject = valueObject;
        }

        public decimal Number(string name)
        {
            var value = GetValue(new VariableReference(name));
            return (decimal) value;
        }

        private FieldInfo GetField(object parentbject, string name)
        {
            var type = parentbject.GetType();
            var field = type.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (field == null) throw new InvalidOperationException($"Couldn't find field: '{name}' on type: '{type.Name}'");
            return field;
        }

        public object GetValue(VariableReference expectedVariable)
        {
            var currentReference = expectedVariable;
            var currentValue = GetField(valueObject, expectedVariable.ParentIdentifier).GetValue(valueObject);
            while (currentReference.HasChildIdentifiers)
            {
                currentReference = currentReference.ChildrenReference();
                currentValue = GetField(currentValue, currentReference.ParentIdentifier).GetValue(currentValue);
            }

            return currentValue;
        }
    }


    public class VariableReference
    {
        public string[] Path { get; }
        public string ParentIdentifier => Path[0];
        public bool HasChildIdentifiers => Path.Length > 1;
        public int Parts => Path.Length;

        public VariableReference(string variableName)
        {
            if (variableName == null) throw new ArgumentNullException(nameof(variableName));
            Path = new[] { variableName };
        }

        public VariableReference(string[] variablePath)
        {
            Path = variablePath ?? throw new ArgumentNullException(nameof(variablePath));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var value in Path)
            {
                if (builder.Length > 0)
                {
                    builder.Append('.');
                }
                builder.Append(value);
            }
            return builder.ToString();
        }

        public VariableReference ChildrenReference()
        {
            var parts = Path[1..];
            return new VariableReference(parts);
        }
    }
}