using Microsoft.AspNetCore.Mvc;
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
    }
}
