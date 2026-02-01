namespace Lexy.Compiler.Language.Symbols;

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

    public Position AddEndColumn(int amount) => new(LineNumber, Column + amount);
}
