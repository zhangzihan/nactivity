using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sys.Workflow;

namespace BpmnWebTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowUsersController : ControllerBase
    {
        private readonly IApplicationLifetime applicationLifetime;

        public WorkflowUsersController(IApplicationLifetime applicationLifetime)
        {
            this.applicationLifetime = applicationLifetime;
        }

        // GET api/values
        [HttpPost]
        public Task<JArray> PostAsync()
        {
            JArray users = JArray.FromObject(new [] 
            {
                new { Id = "评审员", Name = "评审员" },
                new { Id = "主办方评审员", Name = "主办方评审员" }
            });

            return Task.FromResult<JArray>(users);
        }
    }
}
