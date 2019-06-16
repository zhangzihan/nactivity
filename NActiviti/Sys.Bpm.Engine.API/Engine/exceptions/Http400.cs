using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sys.Workflow.Exceptions
{
    /// <summary>
    /// Httpp400异常消息
    /// </summary>
    public class Http400 : HttpException
    {
        private IList<HttpException> details;
        /// <summary>
        /// 错误明细
        /// </summary>
        [JsonProperty("details")]
        public override IList<HttpException> Details
        {
            get
            {
                if (details == null)
                {
                    details = new List<HttpException>();
                }
                return details;
            }
            set => details = value;
        }
    }
}
