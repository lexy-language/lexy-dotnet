using System.Collections;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public class FunctionType : VariableType, ITypeWithMembers
    {
        public string Type { get; }
        public Function Function { get; }

        public FunctionType(string type, Function function)
        {
            Type = type;
            Function = function;
        }

        protected bool Equals(FunctionType other)
        {
            return Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((FunctionType)obj);
        }

        public override int GetHashCode()
        {
            return (Type != null ? Type.GetHashCode() : 0);
        }

        public override string ToString() => Type;

        public VariableType MemberType(string name, IValidationContext context)
        {
            return name switch
            {
                Function.ParameterName => new FunctionParametersType(Type),
                Function.ResultsName => new FunctionResultsType(Type),
                _ => null
            };
        }
    }
}