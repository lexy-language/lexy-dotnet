using System;
using System.Text;

namespace Lexy.Tests;

public class VerifyLogging
{
    private readonly StringBuilder stringBuilder = new(Environment.NewLine);

    private int indention;

    public int Errors { get; private set; }

    public override string ToString() => "Errors: " + Errors + "\n" + stringBuilder;

    public void ErrorOccurred()
    {
        Errors++;
    }

    public void AppendLine(string message)
    {
        stringBuilder.AppendLine(message);
    }

    public void LogAssert(bool valid, string message, string title, params object[] args)
    {
        if (valid) return;

        if (indention > 0)
        {
            stringBuilder.Append(new string(' ', indention * 2));
        }

        var titleFormat = string.Format(title, args);
        stringBuilder.AppendLine(titleFormat + message);
        Errors ++;
    }

    public void WithIndentation(Action action)
    {
        indention++;
        action();
        indention--;
    }
}
