using Lexy.Compiler.Parser;
using Lexy.Compiler.Parser.Logging;

namespace Lexy.Tests;

public record ParseResult<T>(T Result, IParserLogger Logger);