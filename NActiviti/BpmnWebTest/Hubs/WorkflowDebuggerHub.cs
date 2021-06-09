using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Sys.Workflow;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Impl.Bpmn.Listener;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Identities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Sys.Workflow.Cloud.Services.Api;
using Sys.Workflow.Exceptions;
using Sys.Workflow.Engine.Impl.Contexts;

namespace BpmnWebTest.Hubs
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowDebuggerHub : Hub
    {
        private readonly WorkflowDebuggerEventListenerProvider workflowDebugger;
        private readonly IHttpContextAccessor httpContextAccessor;

        public WorkflowDebuggerHub(WorkflowDebuggerEventListenerProvider workflowDebugger,
            IHttpContextAccessor httpContextAccessor)
        {
            this.workflowDebugger = workflowDebugger;
            this.httpContextAccessor = httpContextAccessor;
            this.workflowDebugger.AddWorkflowDebuggerEventListener();
        }

        public override Task OnConnectedAsync()
        {
            var user = Authentication.AuthenticatedUser;

            workflowDebugger.Connected(user, Context.ConnectionId);

            return Task.CompletedTask;
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var user = Authentication.AuthenticatedUser;

            workflowDebugger.Disconnected(user, Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }
    }

    public class WorkflowDebuggerEventListenerProvider
    {
        /// <summary>
        /// 调试器事件日志侦听器
        /// </summary>
        class WorkflowDebuggerEventListener : IActivitiEventListener
        {
            /// <inheritdoc />
            public bool FailOnException => false;

            public IHubContext<WorkflowDebuggerHub> HubContext { get; }

            /// <inheritdoc />
            public void OnEvent(IActivitiEvent @event)
            {
                WorkflowDebuggerEvent evt = @event as WorkflowDebuggerEvent;
                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                if ((processEngineConfiguration?.EnableVerboseExecutionTreeLogging).GetValueOrDefault(false) && evt?.Execution is object)
                {
                    string startUserId = evt.Execution.ProcessInstance.StartUserId;
                    if (startUserId is object && users.TryGetValue(startUserId, out HashSet<string> clients))
                    {
                        foreach (var clientId in clients)
                        {
                            this.HubContext.Clients.Client(clientId).SendAsync("loggerReceived", new
                            {
                                Date = DateTime.Now,
                                evt.Execution.ActivityId,
                                evt.ExecutionTrace,
                                Error = evt.Exception?.ToString(),
                                evt.LogLevel
                            });
                        }
                    }
                }
            }

            public WorkflowDebuggerEventListener(IHubContext<WorkflowDebuggerHub> hubContext)
            {
                this.HubContext = hubContext;
            }
        }

        public class SignalRErrorMiddleware
        {
            private readonly RequestDelegate next;
            private readonly IHubContext<WorkflowDebuggerHub> hubContext;
            private readonly ILogger logger;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="next"></param>
            public SignalRErrorMiddleware(RequestDelegate next,
                ILoggerFactory loggerFactory,
                IHubContext<WorkflowDebuggerHub> hubContext)
            {
                this.next = next;
                this.hubContext = hubContext;
                logger = loggerFactory.CreateLogger<SignalRErrorMiddleware>();
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
                        if (ex is WorkflowDebugException wex)
                        {
                            if (users.TryGetValue(wex.UserId, out HashSet<string> clients))
                            {
                                foreach (var clientId in clients)
                                {
                                    await this.hubContext.Clients.Client(clientId).SendAsync("loggerReceived", new
                                    {
                                        Date = DateTime.Now,
                                        wex.Execution?.ActivityId,
                                        Error = ex?.ToString(),
                                        LogLevel = LogLevel.Error
                                    });
                                }
                            }
                        }
                    }
                    catch { }

                    throw;
                }
            }
        }

        private readonly IHubContext<WorkflowDebuggerHub> hubContext;

        private readonly IProcessEngine processEngine;
        private readonly ProcessEngineConfigurationImpl processEngineConfiguration;
        private WorkflowDebuggerEventListener debuggerEventListener;

        private readonly static IDictionary<string, HashSet<string>> users = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        public WorkflowDebuggerEventListenerProvider(IProcessEngine processEngine,
            IHubContext<WorkflowDebuggerHub> hubContext)
        {
            this.hubContext = hubContext;
            this.processEngine = processEngine;

            this.processEngineConfiguration = processEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl;
        }

        internal void Connected(IUserInfo userInfo, string connectionId)
        {
            if (users.TryGetValue(userInfo.Id, out HashSet<string> clients))
            {
                clients.Add(connectionId);
            }
            else
            {
                users[userInfo.Id] = new HashSet<string>(new string[] { connectionId });
            }
        }

        internal void Disconnected(IUserInfo userInfo, string connectionId)
        {
            if (users.TryGetValue(userInfo.Id, out HashSet<string> clients))
            {
                clients.Remove(connectionId);
                if (clients.Count == 0)
                {
                    users.Remove(userInfo.Id);
                }
            }
        }

        public void AddWorkflowDebuggerEventListener()
        {
            if (processEngineConfiguration.EnableEventDispatcher)
            {
                if (debuggerEventListener is null)
                {
                    debuggerEventListener = new WorkflowDebuggerEventListener(hubContext);
                    processEngineConfiguration.EventDispatcher.AddEventListener(debuggerEventListener);
                }
            }
        }
    }

    public static class SignalRErrorMiddlewareExtensions
    {
        public static IApplicationBuilder UseSignalRError(this IApplicationBuilder app)
        {
            app.UseMiddleware<WorkflowDebuggerEventListenerProvider.SignalRErrorMiddleware>();

            return app;
        }
    }
}
