using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessInstanceTasksController
    {
        PagedResources<TaskResource> getTasks(string processInstanceId, Pageable pageable);
    }
}