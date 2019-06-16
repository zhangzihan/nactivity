using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// mvc controller discovery
    /// </summary>
    internal interface IMvcControllerDiscovery
    {
        /// <summary>
        /// get controllers
        /// </summary>
        /// <returns></returns>
        IEnumerable<MvcControllerInfo> GetControllers();
    }
}