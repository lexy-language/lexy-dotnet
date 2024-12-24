using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class CustomVariableDeclarationType : VariableDeclarationType
    {
        public string Type { get; }

        public CustomVariableDeclarationType(string type)
        {
            Type = type;
        }

        protected bool Equals(CustomVariableDeclarationType other) => Type == other.Type;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CustomVariableDeclarationType)obj);
        }

        public override int GetHashCode() => Type != null ? Type.GetHashCode() : 0;

        public override string ToString() => Type;

        public override VariableType CreateVariableType(IValidationContext context)
        {
            var enumDefinition = context.Nodes.GetEnum(Type);
            if (enumDefinition != null)
            {
                return new EnumType(Type, enumDefinition);
            }
            var tableDefinition = context.Nodes.GetTable(Type);
            if (tableDefinition != null)
            {
                return new TableType(Type, tableDefinition);
            }

            return null;
        }
    }
}