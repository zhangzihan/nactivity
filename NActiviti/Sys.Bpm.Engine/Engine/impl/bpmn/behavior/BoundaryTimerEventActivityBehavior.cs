﻿using System;

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
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.asyncexecutor;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.jobexecutor;
    using Sys.Workflow.engine.impl.persistence.entity;

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