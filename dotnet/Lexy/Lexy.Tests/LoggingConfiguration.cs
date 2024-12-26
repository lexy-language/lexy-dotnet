using System;
using System.Globalization;
using System.IO;
using Lexy.Poc.Core;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Parser;
using Lexy.Poc.Core.RunTime;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace Lexy.Poc
{
    public static class LoggingConfiguration
    {
        private const string ParserLogFile = "parser.log";
        private const string CompilerLogFile = "compiler.log";
        private const string ExecutionLogFile = "execution.log";
        private const string TestsLogFile = "tests.log";

        private static readonly string logRun = $"{DateTime.Now:yyyyMMddHHmmss}-lexy-";

        public static ILoggerFactory CreateLoggerFactory()
        {
            var factory = new LoggerFactory();
            factory.AddSerilog();
            return factory;
        }

        public static void ConfigureSerilog()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<ParserLogger>())
                    .WriteTo.File(FullLogFile(ParserLogFile)))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<CompilerContext>())
                    .WriteTo.File(FullLogFile(CompilerLogFile)))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<ExecutionContext>())
                    .WriteTo.File(FullLogFile(ExecutionLogFile)))
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(Matching.FromSource<ParserLogger>())
                    .Filter.ByExcluding(Matching.FromSource<CompilerContext>())
                    .Filter.ByExcluding(Matching.FromSource<ExecutionContext>())
                    .WriteTo.File(FullLogFile(TestsLogFile)))
                .CreateLogger();
        }

        public static void LogFileNames()
        {
            Console.WriteLine("Log Files:");
            Console.WriteLine($"  parser: {FullLogFile(ParserLogFile)}");
            Console.WriteLine($"  compiler: {FullLogFile(CompilerLogFile)}");
            Console.WriteLine($"  execution: {FullLogFile(ExecutionLogFile)}");
            Console.WriteLine($"  tests: {FullLogFile(TestsLogFile)}");
            Console.WriteLine();
        }

        private static string FullLogFile(string fileName) => Path.Combine(LogFilesDirectory(), LogFile(fileName));

        private static string LogFile(string fileName) => logRun + fileName;

        public static void RemoveOldFiles()
        {
            var logFiles = Directory.GetFiles(LogFilesDirectory(), "*.log");

            foreach (var logFile in logFiles)
            {
                var datePart = Path.GetFileName(logFile).Split("-")[0];
                if (DateTime.TryParseExact(datePart, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime value))
                {
                    if (DateTime.Now.Subtract(value).Hours > 1)
                    {
                        File.Delete(logFile);
                    }
                }
            }
        }

        private static string LogFilesDirectory()
        {
            return Directory.GetCurrentDirectory();
        }
    }
}