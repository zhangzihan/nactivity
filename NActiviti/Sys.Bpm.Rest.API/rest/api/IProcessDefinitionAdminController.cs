using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;

namespace Sys.Workflow.Cloud.Services.Rest.Api
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