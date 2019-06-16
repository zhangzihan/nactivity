namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// action information
    /// </summary>
    internal class MvcActionInfo
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id => $"{ControllerId}:{Name}";

        /// <summary>
        /// action name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// display name
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// controller id
        /// </summary>
        public string ControllerId { get; set; }
    }
}