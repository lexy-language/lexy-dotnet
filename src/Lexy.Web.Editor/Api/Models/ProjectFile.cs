namespace Lexy.Web.Editor.Api.Models;

public class ProjectFile
{
    public string Name { get; }
    public string Folder { get; }
    public string Identifier { get; }

    public ProjectFile(string name, string folder, string identifier)
    {
        Name = name;
        Folder = folder;
        Identifier = identifier;
    }
}