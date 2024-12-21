using Lexy.Poc.Core.Language;

namespace Lexy.Poc.Core.Parser
{
    public interface IParserLogger
    {
        void LogInfo(string message);
        void Log(string message, IComponent component = null);
        void Fail(string message, IComponent component = null);

        bool HasErrors();

        bool HasErrorMessage(string expectedError);

        public string FormatMessages();

        bool ComponentHasErrors(IRootComponent component);

        string[] ComponentFailedMessages(IRootComponent component);
        string[] FailedRootMessages();

        void AssertNoErrors();
    }
}