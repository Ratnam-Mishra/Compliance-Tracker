using Microsoft.Extensions.Configuration;
using WebApp.Infrastructure.Helpers;

namespace Schedulers
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
             .AddEnvironmentVariables()
             .Build();

            Configuration.Initialize(config);

            UserComplianceAnalyzer analyzer = new UserComplianceAnalyzer();
            //var uploadDocumentsCheck = analyzer.DocumentsHandler().Result;
            var cleanMsgsCheck = analyzer.CleanMessages().Result;
        }
    }
}
