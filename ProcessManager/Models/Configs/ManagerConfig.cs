using LassoProcessManager.Models.Rules;

namespace ProcessManager.Models.Configs
{
    public class ManagerConfig
    {
        /// <summary>
        /// Indicates to auto apply default profile for processes when no profiles are assigned.
        /// </summary>
        public bool AutoApplyDefaultProfile { get; set; }

        /// <summary>
        /// Default lasso profile.
        /// </summary>
        public string DefaultProfile { get; set; }

        /// <summary>
        /// Available Lasso profiles.
        /// </summary>
        public LassoProfile[] Profiles { get; set; }

        /// <summary>
        /// List of process rules.
        /// </summary>
        public ProcessRule[] ProcessRules { get; set; }

        /// <summary>
        /// List of folders rules.
        /// </summary>
        public FolderRule[] FolderRules { get; set; }
    }
}
