using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.engine;
using Sys.Bpm.Exceptions;
using System;
using System.Threading.Tasks;

namespace Sys.Workflow.cloud.services.api
{
    /// <summary>
    /// 
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public ErrorHandlingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            logger = loggerFactory.CreateLogger<ErrorHandlingMiddleware>();
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
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private void WriteErrorLogger(Exception ex)
        {
            LoggerMessage.Define(LogLevel.Error,
                                 new EventId(7, "服务异常"),
                                 $"工作流发生内部异常{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}")(logger, ex);
        }

        private void WriteBizErrorLogger(Exception ex)
        {
            LoggerMessage.Define(LogLevel.Error,
                                 new EventId(7, "服务异常"),
                                 $"工作流发生业务异常{Environment.NewLine}{ex.Message}{Environment.NewLine}{ex.StackTrace}")(logger, ex);
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
                WriteBizErrorLogger(ex);

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
                WriteErrorLogger(ex);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                return context.Response.WriteAsync(JsonConvert.SerializeObject(new Http500()
                {
                    Message = ex.Message,
                    Target = ex.GetType().Name
                }));
            }
        }
    }
}
