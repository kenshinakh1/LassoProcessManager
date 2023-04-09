using LassoProcessManager.Models.Rules;
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

        private IConfigProvider ConfigProvider { get; set; }
        private ILogProvider LogProvider { get; set; }

        public LassoManager(IConfigProvider configProvider, ILogProvider logProvider)
        {
            ConfigProvider = configProvider;
            LogProvider = logProvider;
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
                LogProvider.Log("Exception with initial setup. Cannot continue. Check for errors.");
            }
            return false;
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
            Dictionary<string, int> successCount = new Dictionary<string, int>();
            foreach (var process in Process.GetProcesses())
            {
                LassoProfile lassoProfile = GetLassoProfileForProcess(process);
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

            LogProvider.Log(string.Join(". ", successCount.Select(e => $"{e.Key}: {e.Value} processes")));
            LogProvider.Log($"{failCount} processes failed to set profile.");
        }

        private bool TrySetProcessProfile(Process process, LassoProfile lassoProfile, out string profileName)
        {
            if (process is null)
            {
                profileName = null;
                return false;
            }

            if (lassoProfile is null)
            {
                LogProvider.Log($"No profile applied on Process '{process.ProcessName}' (ID:{process.Id}).");
                profileName = null;
                return false;
            }

            try
            {   
                process.ProcessorAffinity = (IntPtr)lassoProfile.GetAffinityMask();

                LogProvider.Log($"Applied profile '{lassoProfile.Name}' on Process '{process.ProcessName}' (ID:{process.Id}).");
                profileName = lassoProfile.Name;
                return true;
            }
            catch (Exception ex)
            {
                LogProvider.Log($"Failed to set profile for Process '{process.ProcessName}' (ID:{process.Id}).");
                profileName = null;
                return false;
            }
        }

        private LassoProfile GetLassoProfileForProcess(Process process)
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

                // Delay setting the profile if delay is defined
                if (lassoProfile.DelayMS > 0)
                {
                    await Task.Delay(lassoProfile.DelayMS);
                }

                TrySetProcessProfile(process, lassoProfile, out _);
            }
            catch { }
        }
    }
}
