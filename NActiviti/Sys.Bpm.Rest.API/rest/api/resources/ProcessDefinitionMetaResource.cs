using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
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