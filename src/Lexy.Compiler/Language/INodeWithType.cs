using Lexy.Compiler.Language.TypeSystem;

namespace Lexy.Compiler.Language;

public interface INodeWithType
{
    Type CreateType();
}
