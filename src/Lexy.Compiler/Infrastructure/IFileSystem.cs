using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lexy.Compiler.Parser.Documents;

namespace Lexy.Compiler.Infrastructure;

public interface IFileSystem {

    Task<string[]> ReadAllLines(string fileName);
    Task WriteAllLines(string fileName, IEnumerable<string> lines);

    StreamReader OpenStream(string fileName);

    Task<bool> FileExists(string fileName);
    Task<bool> DirectoryExists(string absoluteFolder);

    Task<string[]> GetDirectoryFiles(string folder, string[] extensions);
    Task<string[]> GetDirectories(string folder);

    string CurrentFolder();

    string GetFileName(string fullFileName);
    string GetDirectoryName(string fileName);
    string GetFullPath(string directoryName);

    string Combine(string fullPath, string fileName);

    bool IsPathRooted(string folder);

    string LogFolders();

    Task<ISourceCodeDocument> CreateFileSourceDocument(IFile file);
    Task<ISourceCodeDocuments> CreateFileSourceDocuments(IFile[] files);
}
