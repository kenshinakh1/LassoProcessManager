﻿using LassoProcessManager.Models;
using System.Diagnostics;

namespace ProcessManager.Models.Configs
{
    public class LassoProfile
    {
        private UInt64 affinityMask;

        public string Name { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// Hex string format of processor affinity mask.
        /// </summary>
        public string AffinityMaskHex { get; set; }

        /// <summary>
        /// Delay in milliseconds for setting profile.
        /// </summary>
        public int DelayMS { get; set; }

        /// <summary>
        /// IO priority
        /// </summary>
        public IoPriority IoPriority { get; set; } = IoPriority.Normal;

        /// <summary>
        /// Process priority
        /// </summary>
        public ProcessPriorityClass ProcessPriority {  get; set; } = ProcessPriorityClass.Normal;

        /// <summary>
        /// Get Parsed affinity mask in UInt64.
        /// </summary>
        /// <returns></returns>
        public UInt64 GetAffinityMask()
        {
            if (affinityMask == 0)
            {
                affinityMask = Convert.ToUInt64(AffinityMaskHex, 16);
            }

            return affinityMask;
        }
    }
}
