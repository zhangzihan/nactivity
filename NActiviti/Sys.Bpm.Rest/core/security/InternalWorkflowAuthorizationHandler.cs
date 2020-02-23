using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Sys.Net.Http;
using Sys.Workflow.Engine.Impl.Identities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sys.Workflow.Cloud.Services.Core
{
    class InternalWorkflowAuthorizationHandler : AuthorizationHandler<IWorkflowAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IWorkflowAuthorizationRequirement requirement)
        {
            context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
