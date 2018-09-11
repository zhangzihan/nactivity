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
namespace org.activiti.engine.impl.bpmn.behavior
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.history;
    using org.activiti.engine.impl.asyncexecutor;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.jobexecutor;
    using org.activiti.engine.impl.persistence.entity;

    [Serializable]
    public class IntermediateCatchTimerEventActivityBehavior : IntermediateCatchEventActivityBehavior
    {

        private const long serialVersionUID = 1L;

        protected internal TimerEventDefinition timerEventDefinition;

        public IntermediateCatchTimerEventActivityBehavior(TimerEventDefinition timerEventDefinition)
        {
            this.timerEventDefinition = timerEventDefinition;
        }

        public override void execute(IExecutionEntity execution)
        {
            IJobManager jobManager = Context.CommandContext.JobManager;

            // end date should be ignored for intermediate timer events.
            ITimerJobEntity timerJob = jobManager.createTimerJob(timerEventDefinition, false, execution, TriggerTimerEventJobHandler.TYPE, TimerEventHandler.createConfiguration(execution.CurrentActivityId, null, timerEventDefinition.CalendarName));

            if (timerJob != null)
            {
                jobManager.scheduleTimerJob(timerJob);
            }
        }

        public override void eventCancelledByEventGateway(IExecutionEntity execution)
        {
            IJobEntityManager jobEntityManager = Context.CommandContext.JobEntityManager;
            IList<IJobEntity> jobEntities = jobEntityManager.findJobsByExecutionId(execution.Id);

            foreach (IJobEntity jobEntity in jobEntities)
            { // Should be only one
                jobEntityManager.delete(jobEntity);
            }

            Context.CommandContext.ExecutionEntityManager.deleteExecutionAndRelatedData(execution, DeleteReason_Fields.EVENT_BASED_GATEWAY_CANCEL, false);
        }


    }

}