using System;
using System.Text;

namespace Lexy.Tests;

public class VerifyLogging
{
    private readonly StringBuilder stringBuilder = new();

    private int indention;

    public bool Errors { get; private set; }

    public override string ToString()
    {
        return stringBuilder.ToString();
    }

    public void ErrorOccured()
    {
        Errors = true;
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
        Errors = true;
    }

    public void WithIndentation(Action action)
    {
        indention++;
        action();
        indention--;
    }
}