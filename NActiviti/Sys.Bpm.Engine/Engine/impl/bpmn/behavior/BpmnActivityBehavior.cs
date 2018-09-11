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

    using org.activiti.engine.@delegate.@event;
    using org.activiti.engine.@delegate.@event.impl;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.persistence.entity;

    /// <summary>
    /// Helper class for implementing BPMN 2.0 activities, offering convenience methods specific to BPMN 2.0.
    /// <para>
    /// This class can be used by inheritance or aggregation.
    /// </para>
    /// </summary>
    [Serializable]
	public class BpmnActivityBehavior
	{

		private const long serialVersionUID = 1L;

		/// <summary>
		/// Performs the default outgoing BPMN 2.0 behavior, which is having parallel paths of executions for the outgoing sequence flow.
		/// <para>
		/// More precisely: every sequence flow that has a condition which evaluates to true (or which doesn't have a condition), is selected for continuation of the process instance. If multiple sequencer
		/// flow are selected, multiple, parallel paths of executions are created.
		/// </para>
		/// </summary>
		public virtual void performDefaultOutgoingBehavior(IExecutionEntity activityExecution)
		{
			performOutgoingBehavior(activityExecution, true, false);
		}

		/// <summary>
		/// dispatch job canceled event for job associated with given execution entity </summary>
		/// <param name="activityExecution"> </param>
		protected internal virtual void dispatchJobCanceledEvents(IExecutionEntity activityExecution)
		{
			if (activityExecution != null)
			{
				IList<IJobEntity> jobs = activityExecution.Jobs;
				foreach (IJobEntity job in jobs)
				{
					if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
					{
						Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
					}
				}

				IList<ITimerJobEntity> timerJobs = activityExecution.TimerJobs;
				foreach (ITimerJobEntity job in timerJobs)
				{
					if (Context.ProcessEngineConfiguration.EventDispatcher.Enabled)
					{
						Context.ProcessEngineConfiguration.EventDispatcher.dispatchEvent(ActivitiEventBuilder.createEntityEvent(ActivitiEventType.JOB_CANCELED, job));
					}
				}
			}
		}

		/// Performs the default outgoing BPMN 2.0 behavior (<seealso cref= <seealso cref="#performDefaultOutgoingBehavior(ExecutionEntity)"/>), but without checking the conditions on the outgoing sequence flow.
		/// <para>
		/// This means that every outgoing sequence flow is selected for continuing the process instance, regardless of having a condition or not. In case of multiple outgoing sequence flow, multiple
		/// parallel paths of executions will be created. </seealso>
		public virtual void performIgnoreConditionsOutgoingBehavior(IExecutionEntity activityExecution)
		{
			performOutgoingBehavior(activityExecution, false, false);
		}

		/// <summary>
		/// Actual implementation of leaving an activity. </summary>
		/// <param name="execution"> The current execution context </param>
		/// <param name="checkConditions"> Whether or not to check conditions before determining whether or not to take a transition. </param>
		/// <param name="throwExceptionIfExecutionStuck"> If true, an <seealso cref="ActivitiException"/> will be thrown in case no transition could be found to leave the activity. </param>
		protected internal virtual void performOutgoingBehavior(IExecutionEntity execution, bool checkConditions, bool throwExceptionIfExecutionStuck)
		{
			Context.Agenda.planTakeOutgoingSequenceFlowsOperation(execution, true);
		}
	}

}