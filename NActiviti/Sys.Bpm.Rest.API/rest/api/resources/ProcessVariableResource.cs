using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;

namespace Sys.Workflow.cloud.services.rest.api.resources
{

    /// <summary>
    /// 流程变量资源描述
    /// </summary>
    public class ProcessVariableResource : Resource<ProcessInstanceVariable>
    {

        /// <summary>
        /// 
        /// </summary>
        public ProcessVariableResource(ProcessInstanceVariable content, params Link[] links) : base(content, links)
        {
        }
    }
}