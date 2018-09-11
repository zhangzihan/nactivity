using org.activiti.cloud.services.rest.api.resources;

namespace org.activiti.cloud.services.rest.api
{
    public interface IProcessDefinitionMetaController
    {
        ProcessDefinitionMetaResource GetProcessDefinitionMetadata(string id);
    }
}