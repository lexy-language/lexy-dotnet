using System;
using System.Globalization;
using System.IO;
using Lexy.Poc.Core;
using Lexy.Poc.Core.Compiler;
using Lexy.Poc.Core.Parser;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Filters;

namespace Lexy.Poc
{
    public static class LoggingConfiguration
    {
        public static ILoggerFactory CreateLoggerFactory()
        {
            var factory = new LoggerFactory();
            factory.AddSerilog();
            return factory;
        }

        public static void ConfigureSerilog()
        {
            var logRun = $"{DateTime.Now:yyyyMMddHHmmss}-lexy-";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<ParserContext>())
                    .WriteTo.File(logRun + "parser.log"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<CompilerContext>())
                    .WriteTo.File(logRun + "compiler.log"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(Matching.FromSource<ExecutionContext>())
                    .WriteTo.File(logRun + "execution.log"))
                .WriteTo.Logger(lc => lc
                    .Filter.ByExcluding(Matching.FromSource<ParserContext>())
                    .Filter.ByExcluding(Matching.FromSource<CompilerContext>())
                    .Filter.ByExcluding(Matching.FromSource<ExecutionContext>())
                    .WriteTo.File(logRun + "tests.log"))
                .CreateLogger();
        }

        public static void RemoveOldFiles()
        {
            var logFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.log");

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
    }
}