using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
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
        Task<Resources<TaskModel>> getTasks(TaskQuery query);

        /// <summary>
        /// 读取任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> getTaskById(string taskId);

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
        /// <returns>下一步表单列表</returns>
        Task<Resources<TaskModel>> NextForm(string userId);

        /// <summary>
        /// 领取任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> claimTask(string taskId);

        /// <summary>
        /// 释放任务,当前处理人如果不想解决该任务,可以将此任务释放掉,后续其他人员可以领取该任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<TaskModel> releaseTask(string taskId);

        /// <summary>
        /// 处理人已经完该该任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="completeTaskCmd">任务完成命令</param>
        /// <returns></returns>
        Task<IActionResult> completeTask(string taskId, CompleteTaskCmd completeTaskCmd);

        /// <summary>
        /// 终止任务
        /// </summary>
        /// <param name="cmd">终止任务命令</param>
        /// <returns></returns>
        Task<IActionResult> terminate(TerminateTaskCmd cmd);

        /// <summary>
        /// 删除任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        Task<IActionResult> deleteTask(string taskId);

        /// <summary>
        /// 创建新的任务
        /// </summary>
        /// <param name="createTaskCmd">创建任务命令</param>
        /// <returns></returns>
        Task<TaskModel> createNewTask(CreateTaskCmd createTaskCmd);

        /// <summary>
        /// 更新任务状态
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="updateTaskCmd">更新任务状态</param>
        /// <returns></returns>
        Task<IActionResult> updateTask(string taskId, UpdateTaskCmd updateTaskCmd);

        /// <summary>
        /// 创建子任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="createSubtaskCmd">创建子任务命令</param>
        /// <returns></returns>
        Task<TaskModel> createSubtask(string taskId, CreateTaskCmd createSubtaskCmd);

        /// <summary>
        /// 追加任务处理人
        /// </summary>
        /// <param name="cmd">追加任务处理人命令</param>
        /// <returns></returns>
        Task<TaskModel[]> appendCountersign(AppendCountersignCmd cmd);

        /// <summary>
        /// 转办任务处理人
        /// </summary>
        /// <param name="cmd">转办任务命令</param>
        /// <returns></returns>
        Task<TaskModel[]> transferTask(TransferTaskCmd cmd);

        /// <summary>
        /// 读取某个任务下的所有子任务
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<Resources<TaskModel>> getSubtasks(string taskId);
    }
}