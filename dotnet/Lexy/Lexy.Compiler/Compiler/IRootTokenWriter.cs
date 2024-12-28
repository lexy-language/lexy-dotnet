using Lexy.Compiler.Compiler.CSharp;
using Lexy.Compiler.Language;

namespace Lexy.Compiler.Compiler;

internal interface IRootTokenWriter
{
    GeneratedClass CreateCode(IRootNode generateNode);
}