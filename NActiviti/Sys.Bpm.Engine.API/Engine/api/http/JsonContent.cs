using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Net.Http
{

    /// <summary>
    /// 
    /// </summary>
    public class JsonContent : StringContent
    {

        /// <summary>
        /// 
        /// </summary>
        public JsonContent(object data) :
            base(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
        {

        }
    }
}
