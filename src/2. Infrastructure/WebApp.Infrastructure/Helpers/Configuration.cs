using Microsoft.Extensions.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace WebApp.Infrastructure.Helpers
{
    public static class Configuration
    {
        private static IConfiguration? _configuration;

        public static void Initialize(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static string? GetAppSetting(string keyName)
        {
            string keyValue = string.Empty;
            if (_configuration != null)
            {
                keyValue = GetConfigValueUsingIConfiguration(keyName);
            }
            else
            {
                keyValue = GetConfigValueUsingConfigurationManager(keyName);
            }
            return keyValue;
        }

        private static string? GetConfigValueUsingIConfiguration(string keyName)
        {
            return _configuration[keyName];
        }

        private static string? GetConfigValueUsingConfigurationManager(string keyName)
        {
            return ConfigurationManager.AppSettings[keyName];
        }
    }
}
