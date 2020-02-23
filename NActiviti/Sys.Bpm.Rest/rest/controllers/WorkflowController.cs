using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sys.Workflow.Cloud.Services.Rest.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Rest.Controllers
{
    [Authorize(Policy = WorkflowConstants.WORKFLOW_AUTHORIZE_POLICY)]
    public class WorkflowController : ControllerBase
    {
    }
}
