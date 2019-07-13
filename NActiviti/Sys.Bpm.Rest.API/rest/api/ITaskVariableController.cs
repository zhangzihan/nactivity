using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
{
    /// <summary>
    /// 流程任务变量管理RestAPI
    /// </summary>
    public interface ITaskVariableController
    {
        /// <summary>
        /// 获取任务变量
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<Resources<TaskVariableResource>> GetVariables(string taskId);

        /// <summary>
        /// 获取任务变量
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="variableName">变量名称</param>
        /// <returns></returns>
        Task<TaskVariableResource> GetVariable(string taskId, string variableName);

        /// <summary>
        /// 仅获取任务变量
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <returns></returns>
        Task<Resources<TaskVariableResource>> GetVariablesLocal(string taskId);

        /// <summary>
        /// 设置任务变量
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="setTaskVariablesCmd">任务变量命令</param>
        /// <returns></returns>
        Task<bool> SetVariables(string taskId, SetTaskVariablesCmd setTaskVariablesCmd); //(required = true)

        /// <summary>
        /// 设置任务本地变量
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="setTaskVariablesCmd">任务变量命令</param>
        /// <returns></returns>
        Task<bool> SetVariablesLocal(string taskId, SetTaskVariablesCmd setTaskVariablesCmd);
    }
}