using LassoProcessManager.Models.Rules;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProcessManager.Models.Configs;
using System.Reflection;

namespace ProcessManager.Providers
{
    public class ConfigProvider : IConfigProvider
    {
        private const string ConfigFileName = "Config.json";
        private ManagerConfig managerConfig;
        private ILogger logger;

        public ConfigProvider(ILogger logger)
            => this.logger = logger;

        public ManagerConfig GetManagerConfig()
        {
            if (managerConfig != null)
            {
                return managerConfig;
            }

            string configPath = GetConfigFilePath();
            try
            {
                managerConfig = JsonConvert.DeserializeObject<ManagerConfig>(File.ReadAllText(GetConfigFilePath()));
                return managerConfig;
            }
            catch
            {
                logger.Log(LogLevel.Error, $"Failed to load config at '{configPath}'.");
            }

            return null;
        }

        public List<BaseRule> GetRules()
        {
            var rules = new List<BaseRule>();

            // Rule ordering matters here. 
            // Process first, then folders.
            rules.AddRange(managerConfig.ProcessRules);
            rules.AddRange(managerConfig.FolderRules);

            return rules;
        }

        public Dictionary<string, LassoProfile> GetLassoProfiles()
        {
            var lassoProfiles = new Dictionary<string, LassoProfile>();

            // Load lasso profiles
            foreach (var profile in managerConfig.Profiles)
            {
                if (!lassoProfiles.ContainsKey(profile.Name))
                {
                    lassoProfiles.Add(profile.Name, profile);
                }
            }

            return lassoProfiles;
        }

        private string GetConfigFilePath()
            => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), ConfigFileName);
    }
}
