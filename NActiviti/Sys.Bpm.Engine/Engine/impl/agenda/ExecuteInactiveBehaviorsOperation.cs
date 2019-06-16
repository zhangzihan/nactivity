using System.Collections.Generic;

namespace Sys.Workflow.Engine.Impl.Agenda
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Delegate;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow;
    using System;


    /// <summary>
    /// Operation that usually gets scheduled as last operation of handling a <seealso cref="ICommand&lt;T&gt;"/>.
    /// 
    /// Executes 'background' behaviours of executions that currently are in an activity that implements
    /// the <seealso cref="IInactiveActivityBehavior"/> interface.
    /// 
    /// 
    /// </summary>
    public class ExecuteInactiveBehaviorsOperation : AbstractOperation
    {
        private static readonly ILogger<ExecuteInactiveBehaviorsOperation> log = ProcessEngineServiceProvider.LoggerService<ExecuteInactiveBehaviorsOperation>();

        /// <summary>
        /// 
        /// </summary>
        protected internal ICollection<IExecutionEntity> involvedExecutions;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public ExecuteInactiveBehaviorsOperation(ICommandContext commandContext) : base(commandContext, null)
        {
            this.involvedExecutions = commandContext.InvolvedExecutions;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RunOperation()
        {
            try
            {

                /*
                 * Algorithm: for each execution that is involved in this command context,
                 *
                 * 1) Get its process definition
                 * 2) Verify if its process definitions has any InactiveActivityBehavior behaviours.
                 * 3) If so, verify if there are any executions inactive in those activities
                 * 4) Execute the inactivated behavior
                 *
                 */

                foreach (IExecutionEntity executionEntity in involvedExecutions)
                {

                    Process process = ProcessDefinitionUtil.GetProcess(executionEntity.ProcessDefinitionId);
                    ICollection<string> flowNodeIdsWithInactivatedBehavior = new List<string>();
                    foreach (FlowNode flowNode in process.FindFlowElementsOfType<FlowNode>())
                    {
                        if (flowNode.Behavior is IInactiveActivityBehavior)
                        {
                            flowNodeIdsWithInactivatedBehavior.Add(flowNode.Id);
                        }
                    }

                    if (flowNodeIdsWithInactivatedBehavior.Count > 0)
                    {
                        ICollection<IExecutionEntity> inactiveExecutions = commandContext.ExecutionEntityManager.FindInactiveExecutionsByProcessInstanceId(executionEntity.ProcessInstanceId);
                        foreach (IExecutionEntity inactiveExecution in inactiveExecutions)
                        {
                            if (!inactiveExecution.IsActive && flowNodeIdsWithInactivatedBehavior.Contains(inactiveExecution.ActivityId) && !inactiveExecution.Deleted)
                            {

                                FlowNode flowNode = (FlowNode)process.GetFlowElement(inactiveExecution.ActivityId, true);
                                IInactiveActivityBehavior inactiveActivityBehavior = ((IInactiveActivityBehavior)flowNode.Behavior);
                                log.LogDebug($"Found InactiveActivityBehavior instance of class {inactiveActivityBehavior.GetType()} that can be executed on activity '{flowNode.Id}'");
                                inactiveActivityBehavior.ExecuteInactive(inactiveExecution);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}