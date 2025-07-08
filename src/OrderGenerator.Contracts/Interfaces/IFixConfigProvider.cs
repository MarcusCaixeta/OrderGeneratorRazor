using QuickFix;

namespace OrderGenerator.Contracts.Interfaces
{
    public interface IFixConfigProvider
    {
        SessionSettings GetSessionSettings();
        string GetConfigFilePath();

    }
}
