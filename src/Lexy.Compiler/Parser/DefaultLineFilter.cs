namespace Lexy.Compiler.Parser;

public class DefaultLineFilter : ILineFilter
{
    public bool UseLine(string content) => true;
}