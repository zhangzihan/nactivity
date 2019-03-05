using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api.resources
{
    public class TaskVariableResource : Resource<TaskVariable>
    {
        public TaskVariableResource(TaskVariable content, params Link[] links) : base(content, links)
        {
        }
    }

}