using Lexy.Compiler.Parser.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Parser;

public class SourceReference
{
    public int LineNumber { get; }

    public int Column { get; }
    public int EndColumn { get; }

    public string FileName { get; }

    public string SortIndex
    {
        get
        {
            var value = (LineNumber * 100000000 + Column).ToString().PadLeft(16);
            return $"{FileName}/{value}";
        }
    }

    public SourceReference(string fileName, int lineNumber, int column, int endColumn)
    {
        FileName = Assert.NotNull(fileName, nameof(fileName));
        LineNumber = lineNumber;
        Column = column;
        EndColumn = endColumn;
    }

    public override string ToString()
    {
        var suffix = Column != EndColumn ? $"-{EndColumn}" : string.Empty;
        return $"{FileName} ({LineNumber}:{Column}{suffix})";
    }

    public bool Includes(Position position)
    {
        return position.LineNumber == LineNumber
            && position.Column >= Column
            && position.Column <= EndColumn;
    }
}
