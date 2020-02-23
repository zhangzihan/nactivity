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
        /// <summary>
        /// 创建HttpClient请求Access Token
        /// </summary>
        /// <param name="httpClient">httpClient</param>
        /// <param name="clientId">用户id或其它id</param>
        /// <param name="tenantId">组织id</param>
        /// <param name="name">组织id</param>
        /// <param name="email">组织id</param>
        /// <param name="phone">组织id</param>
        void SetHttpClientRequestAccessToken(HttpClient httpClient, string clientId, string tenantId, string name = null, string email = null, string phone = null);

        //// <summary>
        /// 设置http请求access token头
        /// </summary>
        /// <param name="httpClient">HttpClient</param>
        /// <param name="httpContext">Http请求上下文</param>
        Task SetRequestAccessTokenAsync(HttpClient httpClient, HttpContext httpContext = null);

        /// <summary>
        /// 从当前HttpContext上下文中读取用户信息
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<IUserInfo> GetUser(HttpContext context);

        /// <summary>
        /// 获取请求的用户
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        Task<IUserInfo> FromRequestHeaderAsync(HttpContext context);

        /// <summary>
        /// 获取请求的Http Authorization Bearer access_token
        /// </summary>
        /// <param name="context">Http请求上下文</param>
        /// <returns></returns>
        Task<string> GetRequestAccessTokenAsync(HttpContext context);
    }
}