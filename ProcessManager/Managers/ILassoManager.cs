namespace ProcessManager.Managers
{
    public interface ILassoManager : IDisposable
    {
        bool Setup();
        void ResetProfilesForAllProcesses();
    }
}
