using System.Collections.Generic;

namespace Sys.Workflow.cloud.services.core
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