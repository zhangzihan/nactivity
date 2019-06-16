using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.api.runtime.shared.query;
using Sys.Workflow.cloud.services.api.commands;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.cloud.services.rest.api
{
    /// <summary>
    /// 流程实例管理RestAPI
    /// </summary>
    public interface IProcessInstanceController
    {
        /// <summary>
        /// 获取流程实例
        /// </summary>
        /// <param name="query">查询对象</param>
        /// <returns></returns>
        Task<Resources<ProcessInstance>> ProcessInstances(ProcessInstanceQuery query);

        /// <summary>
        /// 启动一个流程
        /// </summary>
        /// <param name="cmds">批量流程启动命令</param>
        /// <returns></returns>
        Task<ProcessInstance[]> Start(StartProcessInstanceCmd[] cmds);

        /// <summary>
        /// 启动一个流程
        /// </summary>
        /// <param name="processDefinitionId">流程启动命令</param>
        /// <param name="businessKey">流程启动命令</param>
        /// <param name="activityId">流程启动命令</param>
        /// <param name="variables">流程启动命令</param>
        /// <returns></returns>
        Task<ProcessInstance> StartByActiviti(string processDefinitionId, string businessKey, string activityId, IDictionary<string, object> variables);

        /// <summary>
        /// 查找一个流程实例
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <returns></returns>
        Task<ProcessInstance> GetProcessInstanceById(string processInstanceId);

        /// <summary>
        /// 获取流程图,未实现
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <returns></returns>
        Task<string> GetProcessDiagram(string processInstanceId);

        /// <summary>
        /// 发送流程信号
        /// </summary>
        /// <param name="cmd">信号命令</param>
        /// <returns></returns>
        Task<ActionResult> SendSignal(SignalCmd cmd);

        /// <summary>
        /// 挂起流程实例
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <returns></returns>
        Task<ProcessInstance> Suspend(string processInstanceId);

        /// <summary>
        /// 激活挂起的流程实例
        /// </summary>
        /// <param name="processInstanceId">流程实例id</param>
        /// <returns></returns>
        Task<ProcessInstance> Activate(string processInstanceId);

        /// <summary>
        /// 终止流程实例
        /// </summary>
        /// <param name="cmd">流程终止命令</param>
        /// <returns></returns>
        Task<ActionResult> Terminate(TerminateProcessInstanceCmd cmd);
    }

}