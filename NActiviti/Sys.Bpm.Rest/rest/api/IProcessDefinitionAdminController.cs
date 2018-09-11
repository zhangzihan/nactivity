using org.activiti.cloud.services.rest.api.resources;
using org.springframework.data.domain;
using org.springframework.hateoas;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessDefinitionAdminController
    {
        PagedResources<ProcessDefinitionResource> GetAllProcessDefinitions(Pageable pageable);
    }
}