using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using org.activiti.engine;
using Sys.Bpm.Exceptions;
using System;
using System.Threading.Tasks;

namespace org.activiti.cloud.services.api
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            if (ex is Http400Exception ex400)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;

                return context.Response.WriteAsync(JsonConvert.SerializeObject(ex400.Http400));
            }
            else if (ex is ActivitiException actex)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 400;

                return context.Response.WriteAsync(JsonConvert.SerializeObject(new Http400()
                {
                    Code = actex.Code,
                    Message = actex.Message,
                    Target = actex.GetType().Name
                }));
            }
            else
            {
                throw ex;
            }
        }
    }
}
