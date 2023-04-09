using LassoProcessManager.Models.Rules;
using ProcessManager.Models.Configs;

namespace ProcessManager.Providers
{
    public interface IConfigProvider
    {
        /// <summary>
        /// Read the config files.
        /// </summary>
        /// <returns></returns>
        ManagerConfig GetManagerConfig();

        /// <summary>
        /// Geth the list of lasso rules.
        /// </summary>
        /// <returns></returns>
        List<BaseRule> GetRules();

        Dictionary<string, LassoProfile> GetLassoProfiles();
    }
}
