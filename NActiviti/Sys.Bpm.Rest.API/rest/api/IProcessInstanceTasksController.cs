using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
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