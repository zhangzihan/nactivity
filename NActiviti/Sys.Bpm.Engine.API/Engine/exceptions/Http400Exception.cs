using System;
using System.Text;

namespace Sys.Bpm.Exceptions
{
    /// <summary>
    /// 400业务异常
    /// </summary>
    public class Http400Exception : Exception
    {
        //{ "error": { "code": "validatorException", "message": "校验失败", "target": "UserLoginCommand", "details": [ { "code": "NotEmptyValidator", "target": "Username", "message": "手机号不能为空" } ] } }
        /// <summary>
        /// 
        /// </summary>
        public Http400 Http400
        {
            get;
            private set;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="http400"></param>
        /// <param name="innerException"></param>
        public Http400Exception(Http400 http400, Exception innerException) : base(http400.Message, innerException)
        {
            this.Http400 = http400;
        }
    }
}
