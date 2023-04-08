namespace ProcessManager.Providers
{
    public interface ILogProvider
    {
        /// <summary>
        /// Simple log function.
        /// </summary>
        /// <param name="message"></param>
        void Log(string message);
    }
}
