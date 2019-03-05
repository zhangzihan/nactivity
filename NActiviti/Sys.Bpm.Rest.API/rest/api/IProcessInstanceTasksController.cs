using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 流程实例任务管理RestAPI
    /// </summary>
    public interface IProcessInstanceTasksController
    {
        /// <summary>
        /// 获取某个流程所有的任务
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <param name="pageable">分页</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> getTasks(string processInstanceId, Pageable pageable);
    }
}