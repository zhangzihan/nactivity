using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sys.Bpm.Exceptions
{
    /// <summary>
    /// Httpp400异常消息
    /// </summary>
    public class Http400
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("code")]
        public string Code { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// Target
        /// </summary>
        [JsonProperty("target")]
        public string Target { get; set; }

        private IList<Http400> details;
        /// <summary>
        /// 错误明细
        /// </summary>
        [JsonProperty("details")]
        public IList<Http400> Details
        {
            get
            {
                if (details == null)
                {
                    details = new List<Http400>();
                }
                return details;
            }
            set => details = value;
        }
    }
}
