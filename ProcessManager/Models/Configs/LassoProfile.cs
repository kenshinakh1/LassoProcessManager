namespace ProcessManager.Models.Configs
{
    public class LassoProfile
    {
        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Hex string format of processor affinity mask.
        /// </summary>
        public string AffinityMaskHex { get; set; }

        /// <summary>
        /// Parsed affinity mask in UInt64.
        /// </summary>
        public UInt64 AffinityMask { get; set; }

        /// <summary>
        /// Delay in milliseconds for setting profile.
        /// </summary>
        public int DelayMS { get; set; }
    }
}
