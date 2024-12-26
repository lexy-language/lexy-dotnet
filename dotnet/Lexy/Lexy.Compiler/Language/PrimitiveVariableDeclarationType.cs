using System;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public sealed class PrimitiveVariableDeclarationType : VariableDeclarationType
    {
        public static PrimitiveVariableDeclarationType Boolean => new PrimitiveVariableDeclarationType(TypeNames.Boolean);
        public static PrimitiveVariableDeclarationType String => new PrimitiveVariableDeclarationType(TypeNames.String);
        public static PrimitiveVariableDeclarationType Number => new PrimitiveVariableDeclarationType(TypeNames.Number);
        public static PrimitiveVariableDeclarationType DateTime => new PrimitiveVariableDeclarationType(TypeNames.Date);

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