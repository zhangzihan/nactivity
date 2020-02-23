using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Core
{
    public class WorkflowAuthorizePolicy<T> where T : IWorkflowAuthorizationRequirement
    {
        public T WorkflowAuthorizationRequirement { get; set; }

        public AuthorizationHandler<T> AuthorizationHandler { get; set; }
    }
}
