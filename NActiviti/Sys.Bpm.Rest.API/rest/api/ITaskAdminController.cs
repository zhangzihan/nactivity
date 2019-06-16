using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
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