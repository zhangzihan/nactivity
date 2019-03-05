using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
{

    public class ProcessDefinitionMetaResource : Resource<ProcessDefinitionMeta>
    {
        public ProcessDefinitionMetaResource(ProcessDefinitionMeta content, params Link[] links) : base(content, links)
        {
        }
    }
}