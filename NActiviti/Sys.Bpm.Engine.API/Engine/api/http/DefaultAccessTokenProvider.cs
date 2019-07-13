using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    /// <inheritdoc />
    public class DefaultAccessTokenProvider : IAccessTokenProvider
    {
        private readonly ILogger<DefaultAccessTokenProvider> logger;

        private readonly string WORKFLOW_CLIENT_ID = "f3fbab3f-3d91-4eed-a63c-93407f4f7538";

        public DefaultAccessTokenProvider(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<DefaultAccessTokenProvider>();
        }

        /// <inheritdoc />
        public void SetHttpClientRequestAccessToken(HttpClient httpClient, string clientId, string tenantId, string name = null, string email = null, string phone = null)
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            string accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(new UserInfo
            {
                Id = string.IsNullOrWhiteSpace(clientId) ? WORKFLOW_CLIENT_ID : clientId,
                FullName = name,
                Email = email,
                Phone = phone,
                TenantId = tenantId
            }));

            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);
        }

        /// <inheritdoc />
        public Task SetRequestAccessTokenAsync(HttpClient httpClient, HttpContext httpContext = null)
        {
            if (httpClient.DefaultRequestHeaders.Authorization != null)
            {
                return Task.CompletedTask;
            }

            string accessToken = null;
            if (Authentication.AuthenticatedUser != null)
            {
                accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(Authentication.AuthenticatedUser));
            }
            else if (httpContext != null)
            {
                accessToken = httpContext.Request.Headers["Authorization"].ToString()?.Split(' ')[1];
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                accessToken = WebUtility.UrlEncode(JsonConvert.SerializeObject(new
                {
                    Id = WORKFLOW_CLIENT_ID
                }));
            }

            httpClient.DefaultRequestHeaders.Remove("Authorization");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Bearer " + accessToken);

            return Task.CompletedTask;
        }


        /// <summary>
        /// 获取请求的用户
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        public Task<IUserInfo> FromRequestHeaderAsync(HttpContext context)
        {
            context.Request.Headers.TryGetValue(Enum.GetName(typeof(HttpRequestHeader), HttpRequestHeader.Authorization), out StringValues authHeader);

            string token = authHeader.ToString();
            string[] tokens = token.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IUserInfo user = null;
            try
            {
                if (tokens[0] == "Bearer")
                {
                    user = JsonConvert.DeserializeObject<UserInfo>(WebUtility.UrlDecode(tokens[1]));
                }

                Authentication.AuthenticatedUser = user;

                return Task.FromResult<IUserInfo>(user);
            }
            catch (IndexOutOfRangeException ex)
            {
                //context.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
                MemoryStream ms = new MemoryStream();
                context.Request.Body.CopyTo(ms);
                context.Request.Body.Flush();
                ms.Seek(0, SeekOrigin.Begin);
                byte[] data = new byte[ms.Length];
                ms.Read(data, 0, data.Length);
                string str = Encoding.UTF8.GetString(data);
                ms.Seek(0, SeekOrigin.Begin);
                context.Request.Body = ms;

                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(tokens[1] + "\t" + ex.Message);

                throw new AuthenticationException("身份验证失败", ex);
            }
        }


        /// <summary>
        /// 获取请求的Http Authorization Bearer access_token
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        public Task<string> GetRequestAccessTokenAsync(HttpContext context)
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
}
