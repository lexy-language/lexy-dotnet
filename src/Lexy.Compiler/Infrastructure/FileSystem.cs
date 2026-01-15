using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lexy.Compiler.Infrastructure;

public class FileSystem : IFileSystem
{
    public Task<string[]> ReadAllLines(string fileName)
    {
        return File.ReadAllLinesAsync(fileName);
    }

    public Task<bool> FileExists(string fileName)
    {
        return Task.FromResult(Path.Exists(fileName));
    }

    public Task<bool> DirectoryExists(string directory)
    {
        return Task.FromResult(Directory.Exists(directory));
    }
    public Task<string[]> GetDirectoryFiles(string folder, string[] extensions)
    {
        var files = Directory.GetFiles(folder)
            .Where(file => extensions.Any(file.EndsWith))
            .ToArray();
        return Task.FromResult(files);
    }

    public Task<string[]> GetDirectories(string folder)
    {
        var directories = Directory.GetDirectories(folder);
        return Task.FromResult(directories);
    }
    public string GetFileName(string fileName)
    {
        return Path.GetFileName(fileName);
    }

    public string GetDirectoryName(string fileName)
    {
        return Path.GetDirectoryName(fileName);
    }

    public string GetFullPath(string fileName)
    {
        return Path.GetFullPath(fileName)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public string Combine(string fullPath, string fileName)
    {
        return Path.Combine(fullPath, fileName);
    }

    public bool IsPathRooted(string folder) => Path.IsPathRooted(folder);

    public string LogFolders() => throw new System.NotImplementedException();

    public string CurrentFolder() => Directory.GetCurrentDirectory();
}
