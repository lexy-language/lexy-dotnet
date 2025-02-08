namespace Lexy.Compiler.Parser;

public interface ILexyParser
{
    ParserResult ParseFile(string fileName, ParseOptions options = null);
    ParserResult Parse(string[] code, string fileName, ParseOptions options = null);
}