using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Sys.Workflow;

namespace Sys.Net.Http
{
    /// <summary>
    /// HttpClientProxy access_token 构建器
    /// </summary>
    public interface IAccessTokenProvider
    {
        //// <summary>
        /// 设置http请求access token头
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        /// <param name="httpContext">Http请求上下文</param>
        void SetRequestAccessToken(HttpClient httpClient, HttpContext httpContext = null);

        /// <summary>
        /// 获取请求的用户
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        Task<IUserInfo> FromRequestHeader(HttpContext context);

        /// <summary>
        /// 获取请求的Http Authorization Bearer access_token
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        Task<string> GetRequestAccessToken(HttpContext context);
    }
}