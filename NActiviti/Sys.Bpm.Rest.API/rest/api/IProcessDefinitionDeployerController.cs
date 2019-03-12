using Microsoft.AspNetCore.Mvc;
using org.activiti.bpmn.model;
using org.activiti.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 流程定义部署RestAPI
    /// </summary>
    public interface IProcessDefinitionDeployerController
    {
        /// <summary>
        /// 部署流程定义
        /// </summary>
        /// <param name="deployer">流程定义部署Model</param>
        /// <returns></returns>
        Task<Deployment> Deploy(ProcessDefinitionDeployer deployer);

        /// <summary>
        /// 保存为草稿
        /// </summary>
        /// <param name="deployer">流程定义部署model</param>
        /// <returns></returns>
        Task<Deployment> Save(ProcessDefinitionDeployer deployer);

        /// <summary>
        /// 移除流程部署定义
        /// </summary>
        /// <param name="deployId">流程定义部署id</param>
        /// <returns></returns>
        Task<IActionResult> Remove(string deployId);

        /// <summary>
        /// 查询最终部署的流程
        /// </summary>
        /// <param name="queryObj">查询对象</param>
        /// <returns></returns>
        Task<Resources<Deployment>> Latest(DeploymentQuery queryObj);

        /// <summary>
        /// 查询所有的部署流程
        /// </summary>
        /// <param name="queryObj">查询对象</param>
        /// <returns></returns>
        Task<Resources<Deployment>> AllDeployments(DeploymentQuery queryObj);

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

        Task<Deployment> Draft(string tenantId, string name);
    }
}
