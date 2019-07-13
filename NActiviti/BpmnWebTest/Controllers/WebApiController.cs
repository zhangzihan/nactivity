using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BpmnWebApiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebApiController : ControllerBase
    {

        [HttpGet("noparameter")]
        public object Get([FromQuery]string businessKey)
        {
            return businessKey;
        }

        [HttpGet("withparameter/{data}")]
        public object Get(int data, [FromQuery]string businessKey)
        {
            return data.ToString() + businessKey;
        }

        [HttpPost("noparameter")]
        public object Post([FromQuery]string businessKey)
        {
            return businessKey;
        }

        [HttpPost("withparameter")]
        public object Post(string[] data, [FromQuery]string businessKey)
        {
            return string.Join(@"\r\n", data) + businessKey;
        }

        [HttpPost("/Api/Party/UserGroup/GetGroupUserList")]
        public Task<JArray> GetGroupUserList([FromBody]dynamic query)
        {
            var idList = query.idList;
            JArray ids = JArray.FromObject(idList);
            var users = new JArray();
            foreach (var id in ids)
            {
                users.Add(JToken.FromObject(new
                {
                    Id = id.ToString(),
                    Name = "流程测试人员"
                }));
            }

            return Task.FromResult(users);
        }

        [HttpGet("taskvariable/{passed}")]
        public bool TaskVariable(bool passed)
        {
            return passed;
        }
    }
}
