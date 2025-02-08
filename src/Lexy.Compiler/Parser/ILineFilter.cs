namespace Lexy.Compiler.Parser;

public interface ILineFilter
{
    bool UseLine(string content);
}