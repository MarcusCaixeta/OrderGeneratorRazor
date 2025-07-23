using QuickFix;

namespace OrderGenerator.WorkerService.Interfaces
{
    public interface IFixConfigProvider
    {
        SessionSettings GetSessionSettings();
        string GetConfigFilePath();

    }
}
