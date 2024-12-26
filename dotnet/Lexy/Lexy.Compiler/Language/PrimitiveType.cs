using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class PrimitiveType : VariableType
    {
        public static PrimitiveType Boolean => new PrimitiveType(TypeNames.Boolean);
        public static PrimitiveType String => new PrimitiveType(TypeNames.String);
        public static PrimitiveType Number => new PrimitiveType(TypeNames.Number);
        public static PrimitiveType Date => new PrimitiveType(TypeNames.Date);

        public string Type { get; }

        public PrimitiveType(string type)
        {
            Type = type;
        }

        protected bool Equals(PrimitiveType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PrimitiveType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public override string ToString() => Type;
    }
}