using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sys.Workflow.Exceptions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.Options;
using Sys.Workflow;
using Sys.Workflow.Engine.Impl.Identities;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Sys.Net.Http
{
    /// <summary>
    /// http client proxy
    /// </summary>
    public class DefaultHttpClientProxy : IHttpClientProxy
    {
        private HttpClient httpClient;
        protected readonly IAccessTokenProvider accessTokenProvider;
        protected readonly HttpContext httpContext;
        protected readonly ILogger<DefaultHttpClientProxy> logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="accessTokenProvider"></param>
        /// <param name="httpContext"></param>
        public DefaultHttpClientProxy(HttpClient httpClient,
            IAccessTokenProvider accessTokenProvider,
            IHttpContextAccessor httpContextAccessor,
            ILoggerFactory loggerFactory)
        {
            this.httpClient = httpClient;
            this.httpClient.Timeout = TimeSpan.FromMinutes(10);
            this.httpClient.DefaultRequestHeaders.ConnectionClose = false;
            this.accessTokenProvider = accessTokenProvider;
            this.httpContext = httpContextAccessor.HttpContext;
            this.logger = loggerFactory.CreateLogger<DefaultHttpClientProxy>();
        }

        public virtual HttpClient HttpClient
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
        protected virtual async Task<T> PopulateData<T>(HttpResponseMessage response)
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
        public virtual async Task<T> GetAsync<T>(string uri)
        {
            return await GetAsync<T>(uri, CancellationToken.None).ConfigureAwait(false);
        }

        public virtual async Task GetAsync(string uri)
        {
            await GetAsync(uri, CancellationToken.None).ConfigureAwait(false);
        }

        public virtual async Task<T> GetAsync<T>(string uri, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                HttpResponseMessage response = await httpClient.GetAsync(uri, cancellation).ConfigureAwait(false);

                return await PopulateData<T>(response).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        public virtual async Task GetAsync(string uri, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                _ = await httpClient.GetAsync(uri, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        /// <summary>
        /// http client post
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<T> PostAsync<T>(string uri, object data = null)
        {
            return await PostAsync<T>(uri, data, CancellationToken.None).ConfigureAwait(false);
        }

        public virtual async Task<T> PostAsync<T>(string uri, object data, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                HttpResponseMessage response = await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);

                return await PopulateData<T>(response).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n{(data == null ? "" : JsonConvert.SerializeObject(data))}" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        public virtual async Task<T> PostAsync<T>(string uri, HttpContent post, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                HttpResponseMessage response = await httpClient.PostAsync(uri, post, cancellation).ConfigureAwait(false);

                return await PopulateData<T>(response).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        public virtual async Task PostAsync(string uri, object data, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                await httpClient.PostAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n{(data == null ? "" : JsonConvert.SerializeObject(data))}" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        public virtual async Task PostAsync(string uri, HttpContent post, CancellationToken cancellation)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            try
            {
                await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

                await httpClient.PostAsync(uri, post, cancellation).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                sw.Stop();

                logger.LogError($"调用外部服务失败({sw.ElapsedMilliseconds}ms) url={uri}\r\n" + ex.Message + "\r\n" + ex.StackTrace);

                throw;
            }
        }

        public virtual async Task PutAsync(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.PutAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);
        }

        public virtual async Task<T> PutAsync<T>(string uri, object data, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.PutAsync(uri, data == null ? null : new JsonContent(data), cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public virtual async Task DeleteAsync(string uri, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            await httpClient.DeleteAsync(uri, cancellation).ConfigureAwait(false);
        }

        public virtual async Task<T> DeleteAsync<T>(string uri, CancellationToken cancellation)
        {
            await accessTokenProvider.SetRequestAccessTokenAsync(httpClient, httpContext).ConfigureAwait(false);

            HttpResponseMessage response = await httpClient.DeleteAsync(uri, cancellation).ConfigureAwait(false);

            return await PopulateData<T>(response).ConfigureAwait(false);
        }

        public virtual void SetHttpClientRequestAccessToken(string clientId, string tenantId, string name = null, string email = null, string phone = null, bool isSessionHeader = true)
        {
            Authentication.AuthenticatedUser = new UserInfo
            {
                Id = clientId,
                FullName = name,
                Email = email,
                Phone = phone,
                TenantId = tenantId
            };
        }
    }
}
