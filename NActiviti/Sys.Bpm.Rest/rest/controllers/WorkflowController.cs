using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Cloud.Services.Rest.Api;
using Sys.Workflow.Engine.Impl.Identities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{
    [Authorize(Policy = WorkflowConstants.WORKFLOW_AUTHORIZE_POLICY)]
    public class WorkflowController : ControllerBase
    {
        /// <summary>
        /// 获取当前认证上下文中的操作人标识。
        /// </summary>
        /// <returns>当前用户标识；若未建立认证上下文则返回空。</returns>
        protected static string? GetAuthenticatedOperatorId()
        {
            return Authentication.AuthenticatedUser?.Id;
        }

        /// <summary>
        /// 将治理业务异常转换为统一的 Http400 业务异常。
        /// </summary>
        /// <param name="code">错误码。</param>
        /// <param name="message">错误消息。</param>
        /// <param name="target">错误目标。</param>
        /// <param name="innerException">原始异常。</param>
        /// <returns>标准化的 Http400 异常。</returns>
        protected static Http400Exception CreateHttp400Exception(string code, string message, string target, Exception innerException)
        {
            return new Http400Exception(new Http400
            {
                Code = code,
                Message = message,
                Target = target,
                OriginError = innerException.GetType().Name
            }, innerException);
        }
    }
}
