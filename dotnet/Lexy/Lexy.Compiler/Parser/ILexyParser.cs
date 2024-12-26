namespace Lexy.Poc.Core.Parser
{
    public interface ILexyParser
    {
        ParserResult ParseFile(string fileName, bool throwException = true);
        ParserResult Parse(string[] code, string fileName, bool throwException = true);
    }
}