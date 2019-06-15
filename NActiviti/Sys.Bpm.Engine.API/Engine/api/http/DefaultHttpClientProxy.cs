using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sys.Bpm.Exceptions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Options;
using Sys.Workflow;

namespace Sys.Net.Http
{
    /// <summary>
    /// http client proxy
    /// </summary>
    public class DefaultHttpClientProxy : IHttpClientProxy
    {
        private HttpClient httpClient;
        private readonly IAccessTokenProvider accessTokenProvider;
        private readonly HttpContext httpContext;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="accessTokenProvider"></param>
        /// <param name="httpContext"></param>
        public DefaultHttpClientProxy(HttpClient httpClient,
            IAccessTokenProvider accessTokenProvider,
            IHttpContextAccessor httpContextAccessor)
        {
            this.httpClient = httpClient;
            this.accessTokenProvider = accessTokenProvider;
            this.httpContext = httpContextAccessor.HttpContext;
        }

        public HttpClient HttpClient
        {
            get => httpClient;
            set => httpClient = value;
        }

        /// <summary>
        /// 构造HttpResponse数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        protected async Task<T> PopulateData<T>(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.BadRequest && response.Content != null)
            {
                Http400 h400 = JsonConvert.DeserializeObject<Http400>(await response.Content.ReadAsStringAsync().ConfigureAwait(false));

                throw new Http400Exception(h400, null);
            }

            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                return default;
            }

            if (response is T res)
            {
                return res;
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                //TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                //TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync().ConfigureAwait(false), settings);
        }

        /// <summary>
        /// http client get
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string uri)
        {
            return await GetAsync<T>(uri, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task GetAsync(string uri)
        {
            await GetAsync(uri, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(string uri, CancellationToken cancellation)
        {
            await this.accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.GetAsync(uri, cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public async Task GetAsync(string uri, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.GetAsync(uri, cancellation).ConfigureAwait(false);
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
            return await PostAsync<T>(uri, data, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public async Task<T> PostAsync<T>(string uri, HttpContent post, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.PostAsync(uri, post, cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public async Task PostAsync(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);
        }

        public async Task PostAsync(string uri, HttpContent post, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.PostAsync(uri, post, cancellation).ConfigureAwait(false);
        }

        public async Task PutAsync(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.PutAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);
        }

        public async Task<T> PutAsync<T>(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.PutAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public async Task DeleteAsync(string uri, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.DeleteAsync(uri, cancellation).ConfigureAwait(false);
        }

        public async Task<T> DeleteAsync<T>(string uri, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.DeleteAsync(uri, cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public void SetHttpClientRequestAccessToken(string clientId, string tenantId)
        {
            accessTokenProvider.SetHttpClientRequestAccessToken(httpClient, clientId, tenantId);
        }
    }
}
