namespace Lexy.Compiler.Language;

public class IncludeState
{
    public bool IsProcessed { get; private set; }

    public IncludeState(bool isProcessed)
    {
        IsProcessed = isProcessed;
    }

    public void SetProcessed()
    {
        IsProcessed = true;
    }
}