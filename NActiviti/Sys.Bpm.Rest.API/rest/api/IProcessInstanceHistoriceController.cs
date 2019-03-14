using Microsoft.AspNetCore.Mvc;
using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.commands;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
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
    }
}