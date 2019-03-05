using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 工作流定义RestAPI
    /// </summary>
    public interface IProcessDefinitionController
    {
        /// <summary>
        /// 获取工作流定义的最终版本
        /// </summary>
        /// <param name="pageable"></param>
        /// <returns></returns>
        Task<Resources<ProcessDefinition>> LatestProcessDefinitions(ProcessDefinitionQuery queryObj);

        /// <summary>
        /// 获取所有工作流定义
        /// </summary>
        /// <param name="pageable">分页</param>
        /// <returns></returns>
        Task<Resources<ProcessDefinition>> ProcessDefinitions(ProcessDefinitionQuery queryObj);

        /// <summary>
        /// 查找工作流
        /// </summary>
        /// <param name="id">流程模型id</param>
        /// <returns></returns>
        Task<ProcessDefinition> GetProcessDefinition(string id);

        /// <summary>
        /// 查找流程定义XML描述
        /// </summary>
        /// <param name="id">流程模型id</param>
        /// <returns></returns>
        Task<string> GetProcessModel(string id);

        /// <summary>
        /// 查找流程定义模型
        /// </summary>
        /// <param name="id">流程模型id</param>
        /// <returns></returns>
        Task<BpmnModel> GetBpmnModel(string id);

        /// <summary>
        /// 获取工作流图表,未实现.
        /// </summary>
        /// <param name="id">流程模型id</param>
        /// <returns></returns>
        Task<string> GetProcessDiagram(string id);
    }
}