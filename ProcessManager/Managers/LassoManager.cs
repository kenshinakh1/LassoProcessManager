using ProcessManager.Models.Configs;
using ProcessManager.Providers;
using System.Diagnostics;
using System.Management;

namespace ProcessManager.Managers
{
    public class LassoManager : ILassoManager
    {
        private Dictionary<string, LassoProfile> lassoProfiles = new Dictionary<string, LassoProfile>();
        private Dictionary<string, ProcessProfile> processProfiles = new Dictionary<string, ProcessProfile>();
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

            // Load lasso profiles
            foreach (var profile in config.Profiles)
            {
                if (!lassoProfiles.ContainsKey(profile.Name))
                {
                    // Convert affinitymask hex to UInt64
                    profile.AffinityMask = Convert.ToUInt64(profile.AffinityMaskHex, 16);
                    lassoProfiles.Add(profile.Name, profile);
                }
            }

            // Load process profiles
            foreach (var profile in config.Processes)
            {
                if (!processProfiles.ContainsKey(profile.Name))
                {
                    processProfiles.Add(profile.Name, profile);
                }
            }
        }

        private void SetupProfilesForAllProcesses()
        {
            int failCount = 0;
            Dictionary<string, int> successCount = new Dictionary<string, int>();
            foreach (var process in Process.GetProcesses())
            {
                bool success = TrySetProcessProfile(process, out string profileName);
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

        private bool TrySetProcessProfile(Process process, out string profile)
        {
            if (process == null)
            {
                profile = null;
                return false;
            }

            try
            {
                LassoProfile lassoProfile = GetLassoProfileForProcessName(process.ProcessName);
                
                if (lassoProfile == null)
                {
                    LogProvider.Log($"No profile applied on Process '{process.ProcessName}' (ID:{process.Id}).");
                    profile = null;
                    return false;
                }

                process.ProcessorAffinity = (IntPtr)lassoProfile.AffinityMask;

                LogProvider.Log($"Applied profile '{lassoProfile.Name}' on Process '{process.ProcessName}' (ID:{process.Id}).");
                profile = lassoProfile.Name;
                return true;
            }
            catch
            {
                LogProvider.Log($"Failed to set profile for Process '{process.ProcessName}' (ID:{process.Id}).");
                profile = null;
                return false;
            }
        }

        private LassoProfile GetLassoProfileForProcessName(string name)
        {
            if (processProfiles.ContainsKey(name))
            {
                string profileName = processProfiles[name].Profile;
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

                LassoProfile lassoProfile = GetLassoProfileForProcessName(process.ProcessName);

                // Delay setting the profile if delay is defined
                if (lassoProfile.DelayMS > 0)
                {
                    await Task.Delay(lassoProfile.DelayMS);
                }

                TrySetProcessProfile(process, out _);
            }
            catch { }
        }
    }
}
