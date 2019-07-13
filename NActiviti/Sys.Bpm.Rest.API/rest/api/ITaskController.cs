using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Services.Api.Commands;
using Sys.Workflow.Hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
{
    /// <summary>
    /// 流程任务管理RestAPI
    /// </summary>
    public interface ITaskController
    {
        /// <summary>
        /// 获取所有任务
        /// </summary>
        /// <param name="query">分页</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> GetTasks(TaskQuery query);

        /// <summary>
        /// 读取任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> GetTaskById(string taskId);

        /// <summary>
        /// 我的待办项
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <returns>待办项列表</returns>
        Task<Resources<TaskModel>> MyTasks(string userId);

        /// <summary>
        /// 下一步表单
        /// </summary>
        /// <param name="userId">用户id</param>
        /// <param name="taskId">任务id</param>
        /// <returns>返回用户下一步任务详情</returns>
        //Task<TaskModel> Next(string userId, string taskId);

        /// <summary>
        /// 领取任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> ClaimTask(string taskId);

        /// <summary>
        /// 释放任务,当前处理人如果不想解决该任务,可以将此任务释放掉,后续其他人员可以领取该任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> ReleaseTask(string taskId);

        /// <summary>
        /// 释放任务,当前处理人如果不想解决该任务,可以将此任务释放掉,后续其他人员可以领取该任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> ReleaseTask(ReleaseTaskCmd cmd);

        /// <summary>
        /// 处理人已经完该该任务
        /// </summary>
        /// <param name="completeTaskCmd">任务完成命令</param>
        /// <returns></returns>
        Task<bool> CompleteTask(CompleteTaskCmd completeTaskCmd);

        /// <summary>
        /// 批量完成处理人任务
        /// </summary>
        /// <param name="cmds">任务完成命令</param>
        /// <returns>仅返回错误的任务</returns>
        Task<CompleteTaskCmd[]> CompleteTask(CompleteTaskCmd[] cmds);

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="cmd">终止任务命令</param>
        /// <returns></returns>
        Task<bool> Terminate(TerminateTaskCmd cmd);

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        Task<bool> DeleteTask(string taskId);

        /// <summary>
        /// 创建新的任务
        /// </summary>
        /// <param name="createTaskCmd">创建任务命令</param>
        /// <returns></returns>
        Task<TaskModel> CreateNewTask(CreateTaskCmd createTaskCmd);

        /// <summary>
        /// 更新任务状态
        /// </summary>
        /// <param name="updateTaskCmd">更新任务状态</param>
        /// <returns></returns>
        Task<bool> UpdateTask(UpdateTaskCmd updateTaskCmd);

        /// <summary>
        /// 创建子任务
        /// </summary>
        /// <param name="createSubtaskCmd">创建子任务命令</param>
        /// <returns></returns>
        Task<TaskModel> CreateSubtask(CreateTaskCmd createSubtaskCmd);

        /// <summary>
        /// 追加任务处理人
        /// </summary>
        /// <param name="cmd">追加任务处理人命令</param>
        /// <returns></returns>
        Task<TaskModel[]> AppendCountersign(AppendCountersignCmd cmd);

        /// <summary>
        /// 转办任务处理人
        /// </summary>
        /// <param name="cmd">转办任务命令</param>
        /// <returns></returns>
        Task<TaskModel[]> TransferTask(TransferTaskCmd cmd);

        /// <summary>
        /// 读取某个任务下的所有子任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> GetSubtasks(string taskId);

        /// <summary>
        /// 审批类型任务-同意
        /// </summary>
        /// <param name="cmd" cref="ApprovaleTaskCmd">同意命令</param>
        /// <returns></returns>
        Task<bool> Approvaled(ApprovaleTaskCmd cmd);

        /// <summary>
        /// 审批类型任务-拒绝
        /// </summary>
        /// <param name="cmd" cref="RejectTaskCmd">拒绝命令</param>
        /// <returns></returns>
        Task<bool> Reject(RejectTaskCmd cmd);

        /// <summary>
        /// 审批类型任务-退回到
        /// </summary>
        /// <param name="cmd" cref="ReturnToTaskCmd">退回到</param>
        /// <returns></returns>
        Task<bool> ReturnTo(ReturnToTaskCmd cmd);
    }
}