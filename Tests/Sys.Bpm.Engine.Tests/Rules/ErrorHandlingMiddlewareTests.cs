using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sys.Workflow.Cloud.Services.Api;
using Sys.Workflow.Engine;
using Sys.Workflow.Exceptions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Sys.Workflow.Rules.Tests
{
    /// <summary>
    /// 全局错误处理中间件隔离测试。
    /// </summary>
    public class ErrorHandlingMiddlewareTests
    {
        /// <summary>
        /// 验证 Http400 业务异常会被序列化为标准 400 JSON 响应。
        /// </summary>
        [Fact]
        public async Task Invoke_Should_WriteHttp400Payload_WhenHttp400ExceptionIsThrown()
        {
            var middleware = CreateMiddleware(_ => throw new Http400Exception(new Http400
            {
                Code = "decision-governance-invalid-request",
                Message = "decisionKey is required",
                Target = "SaveDraft",
                OriginError = "ArgumentException"
            }, new ArgumentException("decisionKey is required")));
            var context = CreateHttpContext();

            await middleware.Invoke(context).ConfigureAwait(false);

            var payload = await ReadResponseAsync<Http400>(context).ConfigureAwait(false);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("decision-governance-invalid-request", payload.Code);
            Assert.Equal("SaveDraft", payload.Target);
            Assert.Equal("ArgumentException", payload.OriginError);
            Assert.Equal("decisionKey is required", payload.Message);
        }

        /// <summary>
        /// 验证 Activiti 业务异常会被转换为标准 400 JSON 响应。
        /// </summary>
        [Fact]
        public async Task Invoke_Should_WriteHttp400Payload_WhenActivitiExceptionIsThrown()
        {
            var exception = new ActivitiException("state is invalid")
            {
                Code = "decision-governance-invalid-state"
            };
            var middleware = CreateMiddleware(_ => throw exception);
            var context = CreateHttpContext();

            await middleware.Invoke(context).ConfigureAwait(false);

            var payload = await ReadResponseAsync<Http400>(context).ConfigureAwait(false);
            Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
            Assert.Equal("decision-governance-invalid-state", payload.Code);
            Assert.Equal(nameof(ActivitiException), payload.Target);
            Assert.Equal(nameof(ActivitiException), payload.OriginError);
            Assert.Equal("state is invalid", payload.Message);
        }

        /// <summary>
        /// 验证未知异常会被序列化为标准 500 JSON 响应。
        /// </summary>
        [Fact]
        public async Task Invoke_Should_WriteHttp500Payload_WhenUnexpectedExceptionIsThrown()
        {
            var middleware = CreateMiddleware(_ => throw new InvalidOperationException("unexpected failure"));
            var context = CreateHttpContext();

            await middleware.Invoke(context).ConfigureAwait(false);

            var payload = await ReadResponseAsync<Http500>(context).ConfigureAwait(false);
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal("workflow-internal-server-error", payload.Code);
            Assert.Equal(nameof(InvalidOperationException), payload.Target);
            Assert.Equal(nameof(InvalidOperationException), payload.OriginError);
            Assert.Equal("unexpected failure", payload.Message);
        }

        /// <summary>
        /// 创建待测中间件实例。
        /// </summary>
        /// <param name="next">下一个请求委托。</param>
        /// <returns>错误处理中间件实例。</returns>
        private static ErrorHandlingMiddleware CreateMiddleware(RequestDelegate next)
        {
            return new ErrorHandlingMiddleware(next, LoggerFactory.Create(_ => { }));
        }

        /// <summary>
        /// 创建带内存响应流的 HTTP 上下文。
        /// </summary>
        /// <returns>测试用 HTTP 上下文。</returns>
        private static DefaultHttpContext CreateHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));
            return context;
        }

        /// <summary>
        /// 将响应流反序列化为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="context">HTTP 上下文。</param>
        /// <returns>反序列化后的响应对象。</returns>
        private static async Task<T> ReadResponseAsync<T>(HttpContext context)
        {
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body, Encoding.UTF8, false, leaveOpen: true);
            var content = await reader.ReadToEndAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(content)!;
        }
    }
}
