using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sys.Net.Http
{
    /// <summary>
    /// http client proxy
    /// </summary>
    public class HttpClientProxy : IHttpClientProxy
    {
        private readonly HttpClient httpClient;
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly HttpContext httpContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="accessTokenProvider"></param>
        /// <param name="httpContext"></param>
        public HttpClientProxy(HttpClient httpClient, IAccessTokenProvider accessTokenProvider, HttpContext httpContext)
        {
            this.httpClient = httpClient;
            this.accessTokenProvider = accessTokenProvider;
            this.httpContext = httpContext;
        }

        public HttpClient HttpClient
        {
            get => httpClient;
        }

        /// <summary>
        /// http client post
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string uri, object data = null)
        {
            this.accessTokenProvider.SetRequestAccessToken(httpClient, httpContext);

            HttpResponseMessage response = await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data));

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            string responseData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseData);
        }

        /// <summary>
        /// http client post
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(string uri, object data = null)
        {
            this.accessTokenProvider.SetRequestAccessToken(httpClient, httpContext);

            return await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data));
        }

        /// <summary>
        /// http client get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            this.accessTokenProvider.SetRequestAccessToken(httpClient, httpContext);

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            return response;
        }

        /// <summary>
        /// http client get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string uri)
        {
            this.accessTokenProvider.SetRequestAccessToken(httpClient, httpContext);

            HttpResponseMessage response = await httpClient.GetAsync(uri);

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default(T);
            }

            string responseData = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(responseData);
        }
    }
}
