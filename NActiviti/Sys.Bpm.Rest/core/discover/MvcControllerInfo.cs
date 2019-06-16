using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
    internal class MvcControllerInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public string Id => $"{AreaName}:{Name}";

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// protected actions
        /// </summary>
        public IEnumerable<MvcActionInfo> ProtectedActions { get; set; }

        /// <summary>
        /// actions
        /// </summary>
        public IEnumerable<MvcActionInfo> Actions { get; set; }

        /// <summary>
        /// anonymous actions
        /// </summary>
        public IEnumerable<MvcActionInfo> AnonymousActions { get; set; }

        /// <summary>
        /// route template
        /// </summary>
        public string RouteTemplate { get; set; }
    }
}