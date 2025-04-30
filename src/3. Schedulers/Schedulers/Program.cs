using Microsoft.Extensions.Configuration;
using WebApp.Infrastructure.Helpers;

namespace Schedulers
{
    internal class ComplianceScheduler
    {
        static void Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                Configuration.Initialize(configuration);

                var complianceAnalyzer = new UserComplianceAnalyzer();
                var  documentsResult = complianceAnalyzer.RunVectorCheckOnNewDocuments().Result;
                var communicationComplianceResult = complianceAnalyzer.ProcessAndValidateUserCommunications().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while running the compliance scheduler: {ex.Message}");
            }
        }
    }
}
