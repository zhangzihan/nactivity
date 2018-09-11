using System.Collections.Generic;

namespace org.activiti.engine.impl.agenda
{
    using Microsoft.Extensions.Logging;
    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.@delegate;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using Sys;


    /// <summary>
    /// Operation that usually gets scheduled as last operation of handling a <seealso cref="Command"/>.
    /// 
    /// Executes 'background' behaviours of executions that currently are in an activity that implements
    /// the <seealso cref="IInactiveActivityBehavior"/> interface.
    /// 
    /// 
    /// </summary>
    public class ExecuteInactiveBehaviorsOperation : AbstractOperation
    {
        private static readonly ILogger<ExecuteInactiveBehaviorsOperation> log = ProcessEngineServiceProvider.LoggerService<ExecuteInactiveBehaviorsOperation>();

        protected internal ICollection<IExecutionEntity> involvedExecutions;

        public ExecuteInactiveBehaviorsOperation(ICommandContext commandContext) : base(commandContext, null)
        {
            this.involvedExecutions = commandContext.InvolvedExecutions;
        }

        protected override void run()
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

                Process process = ProcessDefinitionUtil.getProcess(executionEntity.ProcessDefinitionId);
                ICollection<string> flowNodeIdsWithInactivatedBehavior = new List<string>();
                foreach (FlowNode flowNode in process.findFlowElementsOfType<FlowNode>())
                {
                    if (flowNode.Behavior is IInactiveActivityBehavior)
                    {
                        flowNodeIdsWithInactivatedBehavior.Add(flowNode.Id);
                    }
                }

                if (flowNodeIdsWithInactivatedBehavior.Count > 0)
                {
                    ICollection<IExecutionEntity> inactiveExecutions = commandContext.ExecutionEntityManager.findInactiveExecutionsByProcessInstanceId(executionEntity.ProcessInstanceId);
                    foreach (IExecutionEntity inactiveExecution in inactiveExecutions)
                    {
                        if (!inactiveExecution.IsActive && flowNodeIdsWithInactivatedBehavior.Contains(inactiveExecution.ActivityId) && !inactiveExecution.Deleted)
                        {

                            FlowNode flowNode = (FlowNode)process.getFlowElement(inactiveExecution.ActivityId, true);
                            IInactiveActivityBehavior inactiveActivityBehavior = ((IInactiveActivityBehavior)flowNode.Behavior);
                            log.LogDebug($"Found InactiveActivityBehavior instance of class {inactiveActivityBehavior.GetType()} that can be executed on activity '{flowNode.Id}'");
                            inactiveActivityBehavior.executeInactive(inactiveExecution);
                        }
                    }
                }

            }
        }

    }

}