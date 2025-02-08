using System.IO;
using System.Linq;

namespace Lexy.Compiler.Infrastructure;

public interface IFileSystem {

    string[] ReadAllLines(string fileName);

    string GetFileName(string fullFileName);
    string GetDirectoryName(string fileName);
    string GetFullPath(string directoryName);

    string Combine(string fullPath, string fileName);

    bool FileExists(string fileName);
    bool DirectoryExists(string absoluteFolder);

    bool IsPathRooted(string folder);

    string[] GetDirectoryFiles(string folder, string[] extensions);
    string[] GetDirectories(string folder);

    string LogFolders();
}

class FileSystem : IFileSystem
{
    public string[] ReadAllLines(string fileName)
    {
        return File.ReadAllLines(fileName);
    }

    public string GetFileName(string fileName)
    {
        return Path.GetFileName(fileName);
    }

    public string GetDirectoryName(string fileName)
    {
        throw new System.NotImplementedException();
    }

    public string GetFullPath(string fileName)
    {
        return Path.GetFullPath(fileName)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string Combine(string fullPath, string fileName)
    {
        throw new System.NotImplementedException();
    }

    public bool FileExists(string fileName)
    {
        return Path.Exists(fileName);
    }

    public bool DirectoryExists(string directory)
    {
        return Directory.Exists(directory);
    }

    public bool IsPathRooted(string folder)
    {
        return Path.IsPathRooted(folder);
    }

    public string[] GetDirectoryFiles(string folder, string[] extensions)
    {
        return Directory.GetFiles(folder)
            .Where(file => extensions.Any(file.EndsWith))
            .ToArray();
    }

    public string[] GetDirectories(string folder)
    {
        return Directory.GetDirectories(folder);
    }

    public string LogFolders()
    {
        throw new System.NotImplementedException();
    }
}