using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.rest.api.resources;
using org.springframework.hateoas;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.rest.api
{
    /// <summary>
    /// 工作流实例管理RestAPI,流程管理员使用
    /// </summary>
    public interface IProcessInstanceAdminController
    {
        /// <summary>
        /// 获取所有流程实例
        /// </summary>
        /// <param name="query">流程实例查询对象</param>
        /// <returns></returns>
        Task<Resources<ProcessInstance>> GetAllProcessInstances(ProcessInstanceQuery query);
    }
}