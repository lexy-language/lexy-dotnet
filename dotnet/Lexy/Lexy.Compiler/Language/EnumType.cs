using System.Linq;
using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class EnumType : VariableType, ITypeWithMembers
    {
        public string Type { get; }
        public EnumDefinition Enum { get; }

        public EnumType(string type, EnumDefinition enumDefinition)
        {
            Type = type;
            Enum = enumDefinition;
        }

        protected bool Equals(EnumType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EnumType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public override string ToString() => Type;

        public VariableType MemberType(string name)
        {
            return Enum.Members.Any(member => member.Name == name) ? this : null;
        }
    }
}