using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 流程任务管理RestAPI,管理员使用
    /// </summary>
    public interface ITaskAdminController
    {
        /// <summary>
        /// 获取所有有任务
        /// </summary>
        /// <param name="pageable">分页</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> getAllTasks(Pageable pageable);
    }
}