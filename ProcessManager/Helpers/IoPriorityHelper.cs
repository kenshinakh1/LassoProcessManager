using LassoProcessManager.Models;
using System.Diagnostics;

namespace LassoProcessManager.Helpers
{
    public static class IOPriorityHelper
    {
        // P/Invoke for setting I/O priority
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern bool SetPriorityClass(IntPtr handle, uint priorityClass);

        private const uint PROCESS_MODE_BACKGROUND_BEGIN = 0x00100000;
        private const uint PROCESS_MODE_BACKGROUND_END = 0x00200000;

        public static void SetIoPriority(Process process, IoPriority priority)
        {
            SetPriorityClass(process.Handle, (uint)priority);
        }
    }
}
