using Lexy.Compiler.Language;

namespace Lexy.Compiler.Parser;

public interface ITreeValidationVisitor
{
    void Enter(INode node);
    void Leave(INode node);
}