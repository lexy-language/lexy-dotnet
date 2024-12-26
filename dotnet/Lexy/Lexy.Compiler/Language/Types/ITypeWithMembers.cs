using System.Collections;
using Lexy.Compiler.Parser;

namespace Lexy.Compiler.Language.Types
{
    public interface ITypeWithMembers
    {
        VariableType MemberType(string name, IValidationContext context);
    }
}