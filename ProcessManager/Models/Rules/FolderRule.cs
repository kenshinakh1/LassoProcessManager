using System.Diagnostics;

namespace LassoProcessManager.Models.Rules
{
    public class FolderRule : BaseRule
    {
        public string FolderPath { get; set; }

        public override bool IsMatchForRule(Process process)
        {
            try
            {
                return process.MainModule.FileName.StartsWith(FolderPath);
            }
            catch { }
            return false;
        }
    }
}
