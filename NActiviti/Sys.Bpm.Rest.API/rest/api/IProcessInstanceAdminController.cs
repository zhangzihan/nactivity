using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.cloud.services.rest.api
{
    /// <summary>
    /// 流程实例-管理员功能,工作流实例管理RestAPI,流程管理员使用
    /// </summary>
    public interface IProcessInstanceAdminController
    {
        /// <summary>
        /// 获取所有当前运行的流程实例
        /// </summary>
        /// <param name="query">流程实例查询对象</param>
        /// <returns></returns>
        Task<Resources<ProcessInstance>> GetAllProcessInstances(ProcessInstanceQuery query);

        /// <summary>
        /// 获取所有历史流程实例
        /// </summary>
        /// <param name="query">流程实例查询对象</param>
        /// <returns></returns>
        Task<Resources<HistoricInstance>> GetAllProcessHistoriecs(HistoricInstanceQuery query);
    }
}