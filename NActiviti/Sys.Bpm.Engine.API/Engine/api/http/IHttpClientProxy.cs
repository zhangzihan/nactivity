using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpClientProxy
    {
        HttpClient HttpClient { get; set; }

        /// <summary>
        /// 设置客户端请求Accesstoken
        /// </summary>
        /// <param name="clientId">用户id或其它id</param>
        /// <param name="tenantId">组织id</param>
        /// <param name="name">用户名</param>
        /// <param name="email">email</param>
        /// <param name="phone">phone</param>
        /// <param name="isSessionHeader">isSessionHeader</param>
        void SetHttpClientRequestAccessToken(string clientId, string tenantId, string name = null, string email = null, string phone = null, bool isSessionHeader = true);

        /// <summary>
        /// http get proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string uri);

        /// <summary>
        /// http get proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task GetAsync(string uri);

        /// <summary>
        /// http get proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string uri, CancellationToken cancellation);

        /// <summary>
        /// http get proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task GetAsync(string uri, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, object data = null);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, object data, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="post"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, HttpContent post, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="post"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task PostAsync(string uri, HttpContent post, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task PostAsync(string uri, object data, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task PutAsync(string uri, object data, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> PutAsync<T>(string uri, object data, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task DeleteAsync(string uri, CancellationToken cancellation);

        /// <summary>
        /// http post proxy
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellation"></param>
        /// <returns></returns>
        Task<T> DeleteAsync<T>(string uri, CancellationToken cancellation);
    }
}