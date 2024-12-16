using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public class GeneratedClass
    {
        public IRootToken Token { get; }
        public string Ns { get; }
        public string ClassName { get; }
        public string Code { get; }
        public string FullClassName => $"{Ns}.{ClassName}";

        public GeneratedClass(IRootToken token, string @namespace, string className, string code)
        {
            Token = token;
            Ns = @namespace;
            ClassName = className;
            Code = code;
        }
    }
}