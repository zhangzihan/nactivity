using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using org.activiti.engine.impl.identity;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.core
{
    /// <summary>
    /// AccessToken User Provider
    /// </summary>
    public class TokenUserProvider
    {
        private ILogger logger;

        /// <summary>
        /// 用户信息
        /// </summary>
        internal class UserInfo : IUserInfo
        {
            /// <summary>
            /// user id
            /// </summary>
            public string Id { get; set; }

            /// <summary>
            /// user name
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// tenant id
            /// </summary>
            public string TenantId { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logFactory"></param>
        public TokenUserProvider(ILoggerFactory logFactory)
        {
            this.logger = logFactory.CreateLogger<TokenUserProvider>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task<IUserInfo> FromRequestHeader(HttpContext context)
        {
            context.Request.Headers.TryGetValue(Enum.GetName(typeof(HttpRequestHeader), HttpRequestHeader.Authorization), out StringValues authHeader);

            string token = authHeader.ToString();
            string[] tokens = token.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IUserInfo user = null;
            if (tokens[0] == "Bearer")
            {
                try
                {
#if DEBUG
                    user = JsonConvert.DeserializeObject<UserInfo>(WebUtility.UrlDecode(tokens[1]));
#endif
                }
                catch (Exception ex)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug(ex.Message);
                    }
                    throw new AuthenticationException("身份验证失败", ex);
                }
            }
            else
            {
                throw new NotSupportedException("不支持的身份验证类型");
            }

            Authentication.AuthenticatedUser = user;

            return Task.FromResult<IUserInfo>(user);
        }
    }
}
