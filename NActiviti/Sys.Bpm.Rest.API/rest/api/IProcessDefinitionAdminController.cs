using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;

namespace Sys.Workflow.cloud.services.rest.api
{
    /// <summary>
    /// 工作流定义,管理员使用
    /// </summary>
    public interface IProcessDefinitionAdminController
    {
        /// <summary>
        /// 获取所有流程定义列表
        /// </summary>
        /// <param name="pageable">分页</param>
        /// <returns></returns>
        Resources<ProcessDefinition> GetAllProcessDefinitions(Pageable pageable);
    }
}