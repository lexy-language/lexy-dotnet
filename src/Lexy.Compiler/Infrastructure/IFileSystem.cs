using System.Threading.Tasks;

namespace Lexy.Compiler.Infrastructure;

public interface IFileSystem {

    Task<string[]> ReadAllLines(string fileName);

    Task<bool> FileExists(string fileName);
    Task<bool> DirectoryExists(string absoluteFolder);

    Task<string[]> GetDirectoryFiles(string folder, string[] extensions);
    Task<string[]> GetDirectories(string folder);

    string GetFileName(string fullFileName);
    string GetDirectoryName(string fileName);
    string GetFullPath(string directoryName);

    string Combine(string fullPath, string fileName);

    bool IsPathRooted(string folder);

    string LogFolders();
}
