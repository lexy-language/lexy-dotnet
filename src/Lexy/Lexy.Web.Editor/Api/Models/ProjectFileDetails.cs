namespace Lexy.Web.Editor.Api.Models;

public class ProjectFileDetails
{
    public string Name { get; }
    public string Identifier { get; }
    public string Code { get; }

    public ProjectFileDetails(string name, string identifier, string code)
    {
        Name = name;
        Identifier = identifier;
        Code = code;
    }
}