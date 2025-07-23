using OrderGenerator.WorkerService.Interfaces;
using QuickFix;

namespace OrderGenerator.WorkerService.Service
{
    public class FileFixConfigProvider : IFixConfigProvider
    {
        private readonly string _configPath;

        public FileFixConfigProvider(string configPath)
        {
            _configPath = configPath;
        }

        public SessionSettings GetSessionSettings()
        {
            return new SessionSettings(_configPath);
        }

        public string GetConfigFilePath() => _configPath;

    }

}
