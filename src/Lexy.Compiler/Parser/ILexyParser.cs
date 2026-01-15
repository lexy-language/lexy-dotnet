using System.Threading.Tasks;

namespace Lexy.Compiler.Parser;

public interface ILexyParser
{
    Task<ParserResult> ParseFile(string fileName, ParseOptions options = null);
    Task<ParserResult> Parse(string[] code, string fileName, ParseOptions options = null);
}
