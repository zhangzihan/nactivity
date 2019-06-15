using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sys.Bpm.Exceptions
{
    /// <summary>
    /// Httpp异常消息
    /// </summary>
    public class HttpException
    {
        /// <summary>
        /// 错误代码
        /// </summary>
        [JsonProperty("code")]
        public virtual string Code { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        [JsonProperty("message")]
        public virtual string Message { get; set; }

        /// <summary>
        /// Target
        /// </summary>
        [JsonProperty("target")]
        public virtual string Target { get; set; }

        /// <summary>
        /// 原始异常
        /// </summary>
        [JsonProperty("error")]
        public virtual object OriginError { get; set; }

        /// <summary>
        /// 错误明细
        /// </summary>
        [JsonProperty("details")]
        public virtual IList<HttpException> Details
        {
            get;
            set;
        }
    }
}
