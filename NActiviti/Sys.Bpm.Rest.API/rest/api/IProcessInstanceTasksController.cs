using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.cloud.services.rest.api
{
    /// <summary>
    /// 流程实例任务管理RestAPI
    /// </summary>
    public interface IProcessInstanceTasksController
    {
        /// <summary>
        /// 获取某个流程所有的任务,默认不启用分页，返回当期流程实例下的所有任务.
        /// </summary>
        /// <param name="query">流程任务查询对象</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> GetTasks(ProcessInstanceTaskQuery query);
    }
}