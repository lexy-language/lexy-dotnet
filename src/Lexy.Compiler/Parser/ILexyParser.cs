using System.Collections.Generic;
using System.Threading.Tasks;
using Lexy.Compiler.Parser.Documents;

namespace Lexy.Compiler.Parser;

public interface ILexyParser
{
    Task<ParserResult> ParseCode(string fileName, string[] content, ParseOptions options);
    Task<ParserResult> ParseFile(string fileName, ParseOptions options);
    Task<ParserResult> ParseFiles(IEnumerable<string> fileNames, ParseOptions options);
    Task<ParserResult> ParseDocuments(IEnumerable<ISourceCodeDocument> sourceCodeDocuments, ParseOptions options);
}
