using Microsoft.Extensions.Logging;
using ProcessManager.Managers;
using ProcessManager.Providers;
using System.Diagnostics;
using System.Management;

namespace ProcessManager
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var factory = LoggerFactory.Create(builder => builder.AddConsole());
            var logger = factory.CreateLogger("LassoProcessManager");
            var configProvider = new ConfigProvider(logger);
            using var lassoManager = new LassoManager(configProvider, logger);

            logger.Log(LogLevel.Information, "Initializing ProcessManager...");

            lassoManager.Setup();

            logger.Log(LogLevel.Information, "Finished initializing.");

            logger.Log(LogLevel.Information, "Type 'q' to Exit.");

            string input = Console.ReadLine();
            while (input != "q")
            {
                input = Console.ReadLine();
            }

            //lassoManager.ResetProfilesForAllProcesses();
            logger.Log(LogLevel.Information, "Exiting ProcessManager.");
        }
    }
}