using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;

namespace Sys.Workflow.Cloud.Services.Rest.Api.Resources
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