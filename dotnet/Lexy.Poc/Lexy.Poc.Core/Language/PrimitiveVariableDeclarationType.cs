using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class PrimitiveVariableDeclarationType : VariableDeclarationType
    {
        public string Type { get; }

        public PrimitiveVariableDeclarationType(string type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        protected bool Equals(PrimitiveVariableDeclarationType other) => Type == other.Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PrimitiveVariableDeclarationType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public override string ToString() => Type;

        public override VariableType CreateVariableType(IValidationContext context) => new PrimitiveType(Type);
    }
}