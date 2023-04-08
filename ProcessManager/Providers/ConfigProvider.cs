using Newtonsoft.Json;
using ProcessManager.Models.Configs;
using System.Reflection;

namespace ProcessManager.Providers
{
    public class ConfigProvider : IConfigProvider
    {
        private const string ConfigFileName = "Config.json";

        private ILogProvider LogProvider { get; set; }

        public ConfigProvider(ILogProvider logProvider) 
            => this.LogProvider = logProvider;

        public ManagerConfig GetManagerConfig()
        {
            string configPath = GetConfigFilePath();
            try
            {
                return JsonConvert.DeserializeObject<ManagerConfig>(File.ReadAllText(GetConfigFilePath()));
            }
            catch 
            {
                LogProvider.Log($"Failed to load config at '{configPath}'.");
            }

            return null;
        }

        private string GetConfigFilePath()
            => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigFileName);
    }
}
