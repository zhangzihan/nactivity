using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Delegate.Events;
    using Sys.Workflow.Engine.Delegate.Events.Impl;
    using Sys.Workflow.Engine.Impl.Asyncexecutor;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.JobExecutors;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using System.Threading.Tasks;

    /// 
    [Serializable]
    public class BoundaryTimerEventActivityBehavior : BoundaryEventActivityBehavior
    {
        private const long serialVersionUID = 1L;

        protected internal TimerEventDefinition timerEventDefinition;

        public BoundaryTimerEventActivityBehavior(TimerEventDefinition timerEventDefinition, bool interrupting) : base(interrupting)
        {
            this.timerEventDefinition = timerEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            if (!(execution.CurrentFlowElement is BoundaryEvent))
            {
                throw new ActivitiException("Programmatic error: " + this.GetType() + " should not be used for anything else than a boundary event");
            }

            if (timerEventDefinition.TimeDate == "-1" || timerEventDefinition.TimeCycle == "-1" || timerEventDefinition.TimeDuration == "-1")
            {
                ProcessEngineConfigurationImpl processConfig = Context.CommandContext.ProcessEngineConfiguration;

                var element = execution.CurrentFlowElement as BoundaryEvent;

                var beab = new BoundaryEventActivityBehavior(element.CancelActivity);

                beab.Trigger(execution, TriggerTimerEventJobHandler.TYPE, null);

                processConfig.ExecutionEntityManager.Delete(execution);

                //if (element.CancelActivity)
                //{
                //    IExecutionEntityManager executionEntityManager = processConfig.ExecutionEntityManager;
                //    IExecutionEntity attachedRefScopeExecution = executionEntityManager.FindById<IExecutionEntity>(execution.ParentId);

                //    processConfig.ExecutionEntityManager.Delete(attachedRefScopeExecution);
                //    processConfig.ExecutionEntityManager.Delete(execution);
                //}
            }
            else
            {
                IJobManager jobManager = Context.CommandContext.JobManager;
                string timerConfig = TimerEventHandler.CreateConfiguration(execution.CurrentActivityId, timerEventDefinition.EndDate, timerEventDefinition.CalendarName);
                ITimerJobEntity timerJob = jobManager.CreateTimerJob(timerEventDefinition, interrupting, execution, TriggerTimerEventJobHandler.TYPE, timerConfig);

                if (timerJob != null)
                {
                    jobManager.ScheduleTimerJob(timerJob);
                }
            }
        }
    }
}