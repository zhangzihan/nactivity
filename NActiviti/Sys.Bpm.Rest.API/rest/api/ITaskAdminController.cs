using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 流程任务-管理员功能,流程任务管理RestAPI,管理员使用
    /// </summary>
    public interface ITaskAdminController
    {
        /// <summary>
        /// 获取所有有任务
        /// </summary>
        /// <param name="pageable">分页</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> GetAllTasks(Pageable pageable);

        /// <summary>
        /// 重新指派流程节点执行人，该操作由管理员操作。该节点将终止当前所有待办任务.
        /// 重新由该节点处执行流程.
        /// </summary>
        /// <param name="cmd">操作命令</param>
        /// <returns></returns>
        Task<TaskModel[]> ReassignTaskUser(ReassignTaskUserCmd cmd);
    }
}