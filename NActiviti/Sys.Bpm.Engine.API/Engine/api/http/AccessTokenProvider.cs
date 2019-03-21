using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using org.activiti.engine.impl.identity;
using Sys.Workflow;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    /// <inheritdoc />
    public class AccessTokenProvider
    {
        private class InternalAccessTokenProvider : IAccessTokenProvider
        {
            private readonly ILogger<InternalAccessTokenProvider> logger;

            public InternalAccessTokenProvider(ILoggerFactory loggerFactory)
            {
                logger = loggerFactory.CreateLogger<InternalAccessTokenProvider>();
            }

            /// <inheritdoc />
            public void SetRequestAccessToken(HttpClient httpClient, HttpContext httpContext = null)
            {
                string accessToken = httpContext.Request.Headers["Authorization"].ToString()?.Split(' ')[1];

                httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", accessToken);
            }


            /// <summary>
            /// 获取请求的用户
            /// </summary>
            /// <param name="context">Http请求上下文</param>
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


            /// <summary>
            /// 获取请求的Http Authorization Bearer access_token
            /// </summary>
            /// <param name="context">Http请求上下文</param>
            /// <returns></returns>
            public Task<string> GetRequestAccessToken(HttpContext context)
            {
                context.Request.Headers.TryGetValue(Enum.GetName(typeof(HttpRequestHeader), HttpRequestHeader.Authorization), out StringValues authHeader);

                string token = authHeader.ToString();
                string[] tokens = token.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens[0] == "Bearer")
                {
                    return Task.FromResult(tokens[1]);
                }
                else
                {
                    throw new NotSupportedException("不支持的身份验证类型");
                }
            }
        }

        public static IAccessTokenProvider Create(ILoggerFactory loggerFactory)
        {
            return new InternalAccessTokenProvider(loggerFactory);
        }
    }
}
