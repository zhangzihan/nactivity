using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;

namespace Sys.Workflow.Cloud.Services.Rest.Api.Resources
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