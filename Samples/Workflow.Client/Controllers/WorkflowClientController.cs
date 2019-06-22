using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sys.Net.Http;
using Sys.Workflow;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Hateoas;
using Sys.Workflow.Rest.Client;
using Sys.Workflow.Services.Api.Commands;
using Workflow.Client.Models;

namespace Workflow.Client.Controllers
{
    [Route("api/workflow")]
    [ApiController]
    public class WorkflowClientController : ControllerBase
    {
        /// <summary>
        /// 流程key值，用于启动工作流，流程key+tenantid为唯一值
        /// </summary>
        private static readonly string Process_LeaveRequest = "Process_LeaveRequest";

        /// <summary>
        /// 流程实例接口，用于启动流程以及流程状态的管理
        /// </summary>
        private readonly IProcessInstanceController processInstanceClient = null;
        /// <summary>
        /// 流程任务接口，用于管理流程相关的任务
        /// </summary>
        private readonly ITaskController taskClient = null;
        /// <summary>
        /// 流程任务管理员接口，提供一些后台管理员使用的接口API
        /// </summary>
        private readonly ITaskAdminController taskAdminClient = null;
        /// <summary>
        /// 流程部署接口,主要用于从客户端发起流程部署
        /// </summary>
        private readonly IProcessDefinitionDeployerController deployerClient = null;
        /// <summary>
        /// 流程定义接口,管理流程定义，启用或禁用流程定义，启用流程版本.
        /// </summary>
        private readonly IProcessDefinitionController processDefinitionClient = null;

        private readonly IProcessInstanceTasksController processInstanceTaskClient = null;

        private readonly HttpContext httpContext;

        /// <summary>
        /// 从http context上下文获取或设置用户访问信息
        /// </summary>
        private readonly IAccessTokenProvider accessToken;

        public WorkflowClientController(WorkflowHttpClientProxyProvider workflowHttpClientProxy, IAccessTokenProvider accessToken, IHttpContextAccessor httpContextAccessor)
        {
            httpContext = httpContextAccessor.HttpContext;
            this.accessToken = accessToken;
            processInstanceClient = workflowHttpClientProxy.GetProcessInstanceClient();
            taskClient = workflowHttpClientProxy.GetTaskClient();
            taskAdminClient = workflowHttpClientProxy.GetTaskAdminClient();
            deployerClient = workflowHttpClientProxy.GetDefinitionDeployerClient();
            processDefinitionClient = workflowHttpClientProxy.GetProcessDefinitionClient();
            processInstanceTaskClient = workflowHttpClientProxy.GetProcessInstanceTasksClient();
        }

        [HttpGet("users")]
        public IEnumerable<User> Users()
        {
            return MockDatas.Users;
        }

        [HttpPost("submit")]
        public async Task<bool> Submit(LeaveRequest request)
        {
            //处理业务部分内容
            IUserInfo user = await accessToken.FromRequestHeaderAsync(httpContext);

            MockDatas.Requests.Add(request);

            //转换为流程变量
            Dictionary<string, object> variables = JToken.FromObject(request).ToObject<Dictionary<string, object>>();

            //设置流程管理员用户,变量名对应流程中设置的流程变量名.
            variables.Add("AdminUser", new string[] { MockDatas.AdminUser.Id });
            variables.Add("StartUser", user.Id);

            //这块只是演示，获取节点审批人员方法可以参考activiti，也可以看下Engine\imp\bpmn\rules下的几种角色的获取方法
            variables.Add("上级主管", new string[] { MockDatas.Users.FirstOrDefault(x => x.Duty == "上级主管").Id });
            variables.Add("部门经理", new string[] { MockDatas.Users.FirstOrDefault(x => x.Duty == "部门经理").Id });
            variables.Add("HR", new string[] { MockDatas.Users.FirstOrDefault(x => x.Duty == "HR").Id });

            _ = await processInstanceClient.Start(new StartProcessInstanceCmd[]
            {
                new StartProcessInstanceCmd
                {
                    //使用流程key+tenantid启动一个流程
                    ProcessDefinitionKey = Process_LeaveRequest,
                    TenantId = MockDatas.TenantId,
                    BusinessKey = request.Id,
                    Variables = variables
                }
            });

            return true;
        }

        [HttpGet("mytasks")]
        public async Task<Resources<TaskModel>> MyTasks()
        {
            IUserInfo user = await accessToken.FromRequestHeaderAsync(httpContext);

            return await taskClient.MyTasks(user.Id);
        }

        [HttpGet("passed/{id}")]
        public async Task<bool> Passed(string id)
        {
            //处理业务部分内容
            IUserInfo user = await accessToken.FromRequestHeaderAsync(httpContext);

            //提交流程
            await taskClient.CompleteTask(new CompleteTaskCmd
            {
                BusinessKey = id,
                Assignee = user.Id,
                OutputVariables = new WorkflowVariable
                {
                    { "同意", true}
                }
            });

            return true;
        }

        [HttpGet("reject/{id}")]
        public async Task<bool> Reject(string id)
        {
            //处理业务部分内容
            IUserInfo user = await accessToken.FromRequestHeaderAsync(httpContext);

            //提交流程
            await taskClient.CompleteTask(new CompleteTaskCmd
            {
                BusinessKey = id,
                Assignee = user.Id,
                OutputVariables = new WorkflowVariable
                {
                    { "同意", false}
                }
            });

            return true;
        }
    }
}