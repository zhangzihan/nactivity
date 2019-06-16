using System;
using System.Collections.Generic;

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
namespace Sys.Workflow.engine.impl.bpmn.behavior
{

    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.history;
    using Sys.Workflow.engine.impl.asyncexecutor;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence.entity;

    [Serializable]
    public class IntermediateCatchTimerEventActivityBehavior : IntermediateCatchEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal TimerEventDefinition timerEventDefinition;

        public IntermediateCatchTimerEventActivityBehavior(TimerEventDefinition timerEventDefinition)
        {
            this.timerEventDefinition = timerEventDefinition;
        }

        public override void Execute(IExecutionEntity execution)
        {
            IJobManager jobManager = Context.CommandContext.JobManager;

            // end date should be ignored for intermediate timer events.
            ITimerJobEntity timerJob = jobManager.CreateTimerJob(timerEventDefinition, false, execution, TriggerTimerEventJobHandler.TYPE, TimerEventHandler.CreateConfiguration(execution.CurrentActivityId, null, timerEventDefinition.CalendarName));

            if (timerJob != null)
            {
                jobManager.ScheduleTimerJob(timerJob);
            }
        }

        public override void EventCancelledByEventGateway(IExecutionEntity execution)
        {
            IJobEntityManager jobEntityManager = Context.CommandContext.JobEntityManager;
            IList<IJobEntity> jobEntities = jobEntityManager.FindJobsByExecutionId(execution.Id);

            foreach (IJobEntity jobEntity in jobEntities)
            { // Should be only one
                jobEntityManager.Delete(jobEntity);
            }

            Context.CommandContext.ExecutionEntityManager.DeleteExecutionAndRelatedData(execution, DeleteReasonFields.EVENT_BASED_GATEWAY_CANCEL, false);
        }


    }

}