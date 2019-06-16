using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;

namespace Sys.Workflow.Cloud.Services.Rest.Api.Resources
{


    /// <summary>
    /// 
    /// </summary>
    public class ProcessDefinitionMetaResource : Resource<ProcessDefinitionMeta>
    {

        /// <summary>
        /// 流程定义元数据资源描述
        /// </summary>
        public ProcessDefinitionMetaResource(ProcessDefinitionMeta content, params Link[] links) : base(content, links)
        {
        }
    }
}