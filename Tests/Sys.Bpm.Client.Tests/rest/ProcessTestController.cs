using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Cloud.Services.Api.Commands;
using Sys.Workflow.Cloud.Services.Api.Model;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow.Engine.Runtime;
using Sys.Workflow.Engine.Tasks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BpmnTest.Test
{
    [Route("process-test")]
    [ApiController]
    public class ProcessTestController : ControllerBase
    {
        private readonly IProcessEngine processEngine;

        public ProcessTestController(IProcessEngine processEngine)
        {
            this.processEngine = processEngine;
        }

        [HttpGet]
        public void 创建自启动任务([FromServices]IProcessEngine processEngine)
        {

            ProcessEngineConfigurationImpl configuration = processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl;

            // 时间计算
            DateTime now = DateTime.Now;
            // delay为相较当前时间，延时的时间变量
            DateTime target = now.AddMinutes(1);
            // 时间事件声明
            //ITimerJobEntity timer = new TimerJobEntityImpl()
            //{
            //    Id = configuration.IdGenerator.NextId,
            //    Duedate = target,
            //    Exclusive = true,
            //    JobHandlerConfiguration = "Process_eJKiMV6cs",// 这里存入需要启动的流程key
            //    JobHandlerType = TimerStartEventJobHandler.TYPE
            //};

            //configuration.CommandContextFactory.createCommandContext()

            //ITimerJobEntity timer = configuration.TimerJobDataManager.create();
            //timer.Id = configuration.IdGenerator.NextId;
            //timer.Duedate = target;
            //timer.Exclusive = true;
            //timer.JobHandlerConfiguration = "Process_eJKiMV6cs";// 这里存入需要启动的流程key
            //timer.JobHandlerType = TimerStartEventJobHandler.TYPE;

            //configuration.JobManager.scheduleTimerJob(timer);

            IJobEntity timer = configuration.CommandExecutor.Execute<IJobEntity>(new Sys.Workflow.Engine.Impl.Cmd.CreateTimerStartJobCmd("Process_b55g1NTCp", DateTime.Now.AddMinutes(1), "Task_1gfchcb"));

            //processEngine.RepositoryService.
            // 保存作业事件
            //Context.CommandContext.JobEntityManager.insert(timer);
        }

        [HttpPost("注册")]
        public Task<ActionResult> 注册()
        {
            ///调用系统注册逻辑,返回用户信息

            //启动流程
            IProcessInstance instance = processEngine.RuntimeService.StartProcessInstanceByCmd(
                new StartProcessInstanceCmd()
                {
                    ProcessName = "报名注册",
                    TenantId = "cb79f3dd-e84e-49b0-95c2-0bdafc80f09d",
                    Variables = new Dictionary<string, object>()
                    {
                        { "user", new string[]{ Authentication.AuthenticatedUser.Id } }
                    }
                });

            //读取下一个任务表单
            ITask task = processEngine.TaskService.CreateTaskQuery()
                 .SetTaskAssignee(Authentication.AuthenticatedUser.Id)
                 .SetTaskTenantId("cb79f3dd-e84e-49b0-95c2-0bdafc80f09d")
                 .SingleResult();

            return Task.FromResult<ActionResult>(new JsonResult(new
            {
                Id = "3cac98af-de8e-4476-b495-77ddd70ba841",
                TaskId = task.Id,
                task.Name,
                task.FormKey
            }));
        }

        [HttpGet("{taskId}/登录/{hasTicket}")]
        public ActionResult 登录(string taskId, bool hasTicket)
        {
            //系统登录逻辑,判断是否有余票
            //bool hasTicket = true;

            processEngine.TaskService.Complete(taskId, new Dictionary<string, object>
            {
                {"hasTicket", hasTicket }
            });

            //读取下一个任务表单
            ITask task = processEngine.TaskService.CreateTaskQuery()
                 .SetTaskAssignee(Authentication.AuthenticatedUser.Id)
                 .SetTaskTenantId("cb79f3dd-e84e-49b0-95c2-0bdafc80f09d")
                 .SingleResult();

            if (task is null)
            {
                return Ok();
            }

            return new JsonResult(new
            {
                Id = Authentication.AuthenticatedUser.Id,
                TaskId = task.Id,
                task.Name,
                task.FormKey
            });
        }

        [HttpGet("{taskId}/下一步")]
        public ActionResult 下一步(string taskId)
        {
            processEngine.TaskService.Complete(taskId);

            //读取下一个任务表单
            ITask task = processEngine.TaskService.CreateTaskQuery()
                 .SetTaskAssignee(Authentication.AuthenticatedUser.Id)
                 .SetTaskTenantId("cb79f3dd-e84e-49b0-95c2-0bdafc80f09d")
                 .SingleResult();

            if (task is null)
            {
                return Ok();
            }

            return new JsonResult(new
            {
                Id = Authentication.AuthenticatedUser.Id,
                TaskId = task.Id,
                task.Name,
                task.FormKey
            });
        }

        [HttpGet("{taskId}/我要报名")]
        public ActionResult 我要报名(string taskId)
        {
            processEngine.TaskService.Complete(taskId);

            //读取下一个任务表单
            ITask task = processEngine.TaskService.CreateTaskQuery()
                 .SetTaskAssignee(Authentication.AuthenticatedUser.Id)
                 .SetTaskTenantId("cb79f3dd-e84e-49b0-95c2-0bdafc80f09d")
                 .SingleResult();

            if (task is null)
            {
                return Ok();
            }

            return new JsonResult(new
            {
                Id = Authentication.AuthenticatedUser.Id,
                TaskId = task.Id,
                task.Name,
                task.FormKey
            });
        }

        [HttpPost("values")]
        public string[] ServiceWebApiTest(JToken data)
        {
            return new string[] { data["next_user"].ToString() };
        }

        [HttpPost("intvalue")]
        public int ServiceWebApiTest(int data)
        {
            return data;
        }

        [HttpPost("instance")]
        public ProcessInstance ServiceWebApiTest(ProcessInstance process)
        {
            return process;
        }

        [HttpPost("timer-datetime")]
        public DateTime ServiceWebApiTestTimerDateTime()
        {
            return DateTime.Now;
        }

        [HttpPost("/mail/send")]
        public Task<bool> SendMail(dynamic message)
        {
            return Task.FromResult<bool>(true);
        }

        [HttpPost("/wechat/send")]
        public Task<bool> SendWechat(dynamic message)
        {
            return Task.FromResult<bool>(true);
        }

        [HttpPost("/sms/send")]
        public Task<bool> SendSms(dynamic message)
        {
            return Task.FromResult<bool>(true);
        }
    }
}
