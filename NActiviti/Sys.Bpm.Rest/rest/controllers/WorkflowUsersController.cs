using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Sys.Net.Http;
using Sys.Workflow;

namespace BpmnWebTest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowUsersController : ControllerBase
    {
        private readonly IHostApplicationLifetime applicationLifetime;
        private readonly ExternalConnectorProvider externalConnectorProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="applicationLifetime"></param>
        public WorkflowUsersController(IHostApplicationLifetime applicationLifetime,
            ExternalConnectorProvider externalConnectorProvider)
        {
            this.applicationLifetime = applicationLifetime;
            this.externalConnectorProvider = externalConnectorProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // GET api/values
        [HttpPost]
        public Task<JArray> PostAsync([FromBody]dynamic query)
        {
            JArray users;
            if (query is object)
            {
                var idList = query.idList;
                JArray ids = JArray.FromObject(idList);
                users = new JArray();
                foreach (var id in ids)
                {
                    users.Add(JToken.FromObject(new
                    {
                        Id = id.ToString(),
                        Name = "流程测试人员"
                    }));
                }
            }
            else
            {
                users = JArray.FromObject(new[]
                {
                    new { Id = "评审员", Name = "评审员" },
                    new { Id = "主办方评审员", Name = "主办方评审员" }
                });
            }

            return Task.FromResult<JArray>(users);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost("user")]
        public Task<IUserInfo> GetUserAsync(dynamic query)
        {
            string userId = query.id.ToString();

            return Task.FromResult<IUserInfo>(new UserInfo
            {
                Id = userId,
                FullName = "测试用户"
            });
        }
    }
}
