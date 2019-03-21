using System.Net.Http;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    /// <summary>
    /// 
    /// </summary>
    public interface IHttpClientProxy
    {
        HttpClient HttpClient { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> GetAsync(string uri);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        Task<T> GetAsync<T>(string uri);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<T> PostAsync<T>(string uri, object data = null);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> PostAsync(string uri, object data = null);
    }
}