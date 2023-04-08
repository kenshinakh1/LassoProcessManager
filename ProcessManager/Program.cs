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
            ILogProvider logProvider = new LogProvider();
            IConfigProvider configProvider = new ConfigProvider(logProvider);
            using ILassoManager lassoManager = new LassoManager(configProvider, logProvider);

            Console.WriteLine("Initializing ProcessManager...");

            lassoManager.Setup();

            Console.WriteLine("Finished initializing.");

            Console.WriteLine("Type 'q' to Exit.");

            string input = Console.ReadLine();
            while (input != "q")
            {
                input = Console.ReadLine();
            }

            Console.WriteLine("Exiting ProcessManager.");
        }
    }
}