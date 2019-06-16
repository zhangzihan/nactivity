using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sys.Net.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sys.Workflow.engine.impl.bpmn.webservice
{
    public class ServiceWebApiHttpProxy : IServiceWebApiHttpProxy
    {
        private readonly IHttpClientProxy httpProxy;

        internal static readonly string WORKFLOW_CLIENT_ID = "f3fbab3f-3d91-4eed-a63c-93407f4f7538";

        private readonly ILogger logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userServiceProxyBaseUrl">WebApi服务基地址</param>
        public ServiceWebApiHttpProxy(IHttpClientProxy httpProxy,
            ILoggerFactory loggerFactory)
        {
            this.httpProxy = httpProxy;
            logger = loggerFactory.CreateLogger<ServiceWebApiHttpProxy>();
        }

        public HttpClient HttpClient
        {
            get => httpProxy.HttpClient;
        }

        /// <summary>
        /// post 
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <returns></returns>
        public async Task GetAsync(string uri)
        {
            try
            {
                await httpProxy.GetAsync(uri, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                throw;
            }
        }

        /// <summary>
        /// post 
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string uri, CancellationToken cancellationToken)
        {
            try
            {
                return await httpProxy.GetAsync<T>(uri, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                throw;
            }
        }

        /// <summary>
        /// post 
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <returns></returns>
        public async Task PostAsync(string uri, object parameter)
        {
            try
            {
                await httpProxy.PostAsync(uri, parameter, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                throw;
            }
        }

        /// <summary>
        /// post 
        /// </summary>
        /// <param name="parameter">parameter</param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string uri, object parameter, CancellationToken cancellationToken)
        {
            try
            {
                return await httpProxy.PostAsync<T>(uri, parameter, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message, ex);

                throw;
            }
        }
    }
}
