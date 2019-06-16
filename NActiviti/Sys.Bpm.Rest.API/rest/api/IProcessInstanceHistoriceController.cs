using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Api.Runtime.Shared.Query;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api.Resources;
using Sys.Workflow.Hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Rest.Api
{
    /// <summary>
    /// 流程历史实例RestAPI
    /// </summary>
    public interface IProcessInstanceHistoriceController
    {
        /// <summary>
        /// 获取流程历史实例
        /// </summary>
        /// <param name="query">查询对象</param>
        /// <returns>实例列表</returns>
        Task<Resources<HistoricInstance>> ProcessInstances(HistoricInstanceQuery query);

        /// <summary>
        /// 查找一个流程历史实例
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <returns>历史实例</returns>
        Task<HistoricInstance> GetProcessInstanceById(string processInstanceId);

        /// <summary>
        /// 读取流程实例变量列表,流程实例全局变量,作用域范围为整个工作流实例
        /// </summary>
        /// <param name="processInstanceId">流程历史实例id</param>
        /// <param name="taskId">流程实例任务id</param>
        /// <returns></returns>
        Task<Resources<HistoricVariableInstance>> GetVariables(string processInstanceId, string taskId);

        /// <summary>
        /// 读取流程实例变量列表,流程实例全局变量,作用域范围为整个工作流实例
        /// </summary>
        /// <param name="processInstanceId">流程历史实例id</param>
        /// <returns></returns>
        Task<Resources<HistoricVariableInstance>> GetVariables(ProcessVariablesQuery query);
    }
}