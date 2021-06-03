using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Api
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
                try
                {
                    await HandleExceptionAsync(context, ex).ConfigureAwait(false);
                }
                catch { }

                throw;
            }
        }

        private void WriteErrorLogger(HttpContext context, Exception ex)
        {
            string request = null;
            var req = context.Request;
            if (context.Request.Body is object)
            {
                context.Request.EnableBuffering();

                using (StreamReader reader = new(req.Body))
                {
                    reader.BaseStream.Seek(0, SeekOrigin.Begin);
                    request = reader.ReadToEnd();
                }
            }

            string message = $"服务发生异常{Environment.NewLine}" +
                $"Url={req.Scheme}://{req.Host}{req.Path}{req.QueryString}," +
                $"{(request is object ? $"参数={request}" : "")}" +
                $"{(ex is WorkflowDebugException ? ex.InnerException.Message : ex.Message)}{Environment.NewLine}" +
                $"{ex.StackTrace}";

            logger.LogError(message);
        }

        private void WriteBizErrorLogger(Exception ex)
        {
            string message = $"服务发生异常{Environment.NewLine}" +
                $"{ex.Message}{Environment.NewLine}" +
                $"{ex.StackTrace}";

            logger.LogError(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {

            //if (ex is Http400Exception ex400)
            //{
            //    context.Response.ContentType = "application/json";
            //    context.Response.StatusCode = 400;

            //    return context.Response.WriteAsync(JsonConvert.SerializeObject(ex400.Http400));
            //}
            //else 
            if (ex is ActivitiException actex)
            {
                WriteBizErrorLogger(ex);

                //context.Response.ContentType = "application/json";
                //context.Response.StatusCode = 400;

                //return context.Response.WriteAsync(JsonConvert.SerializeObject(new Http400()
                //{
                //    Code = actex.Code,
                //    Message = actex.Message,
                //    Target = actex.GetType().Name
                //}));
            }
            else
            {
                WriteErrorLogger(context, ex);

                //context.Response.ContentType = "application/json";
                //context.Response.StatusCode = 500;

                //return context.Response.WriteAsync(JsonConvert.SerializeObject(new Http500()
                //{
                //    Message = ex.Message,
                //    Target = ex.GetType().Name
                //}));
            }

            return Task.CompletedTask;
        }
    }
}
