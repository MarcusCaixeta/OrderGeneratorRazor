using OrderGenerator.Contracts.Interfaces;
using QuickFix;

namespace OrderGenerator.Infrastructure.Fix
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
