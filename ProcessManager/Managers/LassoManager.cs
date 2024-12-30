using LassoProcessManager.Models.Rules;
using Microsoft.Extensions.Logging;
using ProcessManager.Models.Configs;
using ProcessManager.Providers;
using System.Diagnostics;
using System.Management;

namespace ProcessManager.Managers
{
    public class LassoManager : ILassoManager
    {
        private Dictionary<string, LassoProfile> lassoProfiles;
        private List<BaseRule> rules;
        private ManagerConfig config;
        private ManagementEventWatcher processStartEvent;
        private ILogger logger;

        private IConfigProvider ConfigProvider { get; set; }

        public LassoManager(IConfigProvider configProvider, ILogger logger)
        {
            ConfigProvider = configProvider;
            this.logger = logger;
        }

        public void Dispose()
        {
            if (processStartEvent != null) 
            {
                processStartEvent.EventArrived -= ProcessStartEvent_EventArrived;
                processStartEvent.Dispose();
                processStartEvent = null;
            }
        }

        public bool Setup()
        {
            try
            {
                LoadConfig();
                SetupProfilesForAllProcesses();
                SetupEventWatcher();
            }
            catch
            {
                logger.Log(LogLevel.Error, $"Exception with initial setup. Cannot continue. Check for errors");
            }
            return false;
        }

        public void ResetProfilesForAllProcesses()
        {
            if (TryGetLassoProfile("AllCore", out var profile))
            {
                foreach (var process in Process.GetProcesses())
                {
                    TrySetProcessProfile(process, profile, out string profileName);
                }
            }

            logger.Log(LogLevel.Information, "Reset processes profiles to AllCore.");
        }

        private void LoadConfig()
        {
            config = ConfigProvider.GetManagerConfig();
            rules = ConfigProvider.GetRules();
            lassoProfiles = ConfigProvider.GetLassoProfiles();
        }

        private void SetupProfilesForAllProcesses()
        {
            int failCount = 0;
            var successCount = new Dictionary<string, int>();
            foreach (var process in Process.GetProcesses())
            {
                var lassoProfile = GetLassoProfileForProcess(process);
                bool success = TrySetProcessProfile(process, lassoProfile, out string profileName);

                if (success)
                {
                    if (!successCount.ContainsKey(profileName))
                    {
                        successCount[profileName] = 1;
                    }
                    else
                    {
                        successCount[profileName]++;
                    }
                }
                else
                {
                    failCount++;
                }
            }

            logger.Log(LogLevel.Information, string.Join(". ", successCount.Select(e => $"{e.Key}: {e.Value} processes")));
            logger.Log(LogLevel.Information, $"{failCount} processes failed to set profile.");
        }

        private bool TrySetProcessProfile(Process process, LassoProfile lassoProfile, out string profileName)
        {
            profileName = string.Empty;

            if (process is null || lassoProfile is null)
            {
                //logger.Log(LogLevel.Information, $"No profile applied on Process '{process.ProcessName}' (ID:{process.Id}).");
                return false;
            }

            try
            {   
                process.ProcessorAffinity = (IntPtr)lassoProfile.GetAffinityMask();

                logger.Log(LogLevel.Information, $"Applied profile '{lassoProfile.Name}' on Process '{process.ProcessName}' (ID:{process.Id}).");
                profileName = lassoProfile.Name;
                return true;
            }
            catch
            {
                logger.Log(LogLevel.Error, $"Failed to set profile for Process '{process.ProcessName}' (ID:{process.Id}).");
                return false;
            }
        }

        private bool TryGetLassoProfile(string profileName, out LassoProfile profile)
        {
            if (lassoProfiles.ContainsKey(profileName))
            {
                profile = lassoProfiles[profileName];
                return true;
            }

            profile = null;
            return false;
        }

        private LassoProfile? GetLassoProfileForProcess(Process process)
        {
            var matchingRule = rules.Where(e => e.IsMatchForRule(process)).FirstOrDefault();

            if (matchingRule is { })
            {
                string profileName = matchingRule.Profile;
                if (lassoProfiles.ContainsKey(profileName))
                {
                    return lassoProfiles[profileName];
                }
            }

            if (config.AutoApplyDefaultProfile)
            {
                return lassoProfiles[config.DefaultProfile];
            }

            return null;
        }

        private void SetupEventWatcher()
        {
            processStartEvent = new ManagementEventWatcher("SELECT * FROM Win32_ProcessStartTrace");
            processStartEvent.EventArrived += ProcessStartEvent_EventArrived;
            processStartEvent.Start();
        }

        private async void ProcessStartEvent_EventArrived(object sender, EventArrivedEventArgs e)
        {
            try
            {
                int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessID"].Value);
                var process = Process.GetProcessById(processId);

                LassoProfile lassoProfile = GetLassoProfileForProcess(process);

                if (lassoProfile is not null)
                {
                    // Delay setting the profile if delay is defined
                    if (lassoProfile.DelayMS > 0)
                    {
                        await Task.Delay(lassoProfile.DelayMS);
                    }

                    TrySetProcessProfile(process, lassoProfile, out _);
                }
                else
                {
                    //logger.Log(LogLevel.Information, $"No profile applied on Process '{process.ProcessName}' (ID:{process.Id}).");
                }
            }
            catch { }
        }
    }
}
