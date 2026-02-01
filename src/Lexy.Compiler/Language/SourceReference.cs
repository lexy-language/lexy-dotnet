using System;
using Lexy.Compiler.Language.Symbols;
using Lexy.RunTime;

namespace Lexy.Compiler.Language;

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

    protected bool Equals(SourceReference other)
    {
        return LineNumber == other.LineNumber && Column == other.Column && EndColumn == other.EndColumn && FileName == other.FileName;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((SourceReference)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(LineNumber, Column, EndColumn, FileName);
    }
}
