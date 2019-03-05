using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
{
    public class ProcessVariableResource : Resource<ProcessInstanceVariable>
    {
        public ProcessVariableResource(ProcessInstanceVariable content, params Link[] links) : base(content, links)
        {
        }
    }
}