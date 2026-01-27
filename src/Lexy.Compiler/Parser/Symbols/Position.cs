namespace Lexy.Compiler.Parser.Symbols;

public class Position
{
    public int LineNumber { get; }
    public int Column { get; }

    public Position(int lineNumber, int column)
    {
        LineNumber = lineNumber;
        Column = column;
    }

    public override string ToString()
    {
        return $"{LineNumber}:{Column}";
    }
}
