using Lexy.Poc.Core.Parser;

namespace Lexy.Poc.Core.Language
{
    public class TableType : VariableType, ITypeWithMembers
    {
        public string Type { get; }
        public Table Table { get; }

        public TableType(string type, Table table)
        {
            Type = type;
            Table = table;
        }

        protected bool Equals(TableType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TableType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public override string ToString() => Type;

        public VariableType MemberType(string name)
        {
            return name switch
            {
                "Count" => PrimitiveType.Number,
                _ => null
            };
        }
    }
}