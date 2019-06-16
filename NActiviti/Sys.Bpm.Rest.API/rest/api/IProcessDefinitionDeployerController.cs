using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.bpmn.model;
using Sys.Workflow.cloud.services.api.model;
using org.springframework.hateoas;
using System.Threading.Tasks;

namespace Sys.Workflow.cloud.services.rest.api
{
    /// <summary>
    /// 流程部署-流程发布及管理未发布和已发布流程
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
        Task<ActionResult> Remove(string deployId);

        /// <summary>
        /// 查询最终部署的流程(已发布)
        /// </summary>
        /// <param name="queryObj">查询对象</param>
        /// <returns></returns>
        Task<Resources<Deployment>> Latest(DeploymentQuery queryObj);

        /// <summary>
        /// 查询所有的部署流程(工作流列表已发布/未发布)
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
        /// 查找流程定义XML描述
        /// </summary>
        /// <param name="query">查询对象</param>
        /// <returns></returns>
        Task<string> GetProcessModel(DeploymentQuery query);

        /// <summary>
        /// 查找流程定义模型
        /// </summary>
        /// <param name="id">流程模型id</param>
        /// <returns></returns>
        Task<ActionResult<BpmnModel>> GetBpmnModel(string id);

        /// <summary>
        /// 仅查询草稿,一个流程始终只有一个草稿.
        /// </summary>
        /// <param name="tenantId">租户id</param>
        /// <param name="name">流程名称</param>
        /// <returns>部署定义</returns>
        Task<Deployment> Draft(string tenantId, string name);
    }
}
