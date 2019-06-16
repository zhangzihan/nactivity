using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;

namespace Sys.Workflow.cloud.services.rest.api.resources
{

    /// <summary>
    /// 流程任务变量资源描述
    /// </summary>
    public class TaskVariableResource : Resource<TaskVariable>
    {

        /// <summary>
        /// 
        /// </summary>
        public TaskVariableResource(TaskVariable content, params Link[] links) : base(content, links)
        {
        }
    }

}