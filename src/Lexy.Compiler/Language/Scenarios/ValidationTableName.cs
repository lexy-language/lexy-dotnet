namespace Lexy.Compiler.Language.Scenarios;

public class ValidationTableName
{
    public string Value { get; private set; }

    public void ParseName(string parameter)
    {
        Value = parameter;
    }
}