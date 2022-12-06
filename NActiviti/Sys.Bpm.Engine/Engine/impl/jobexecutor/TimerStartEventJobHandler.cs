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
namespace Sys.Workflow.Engine.Impl.JobExecutors
{
	using Microsoft.Extensions.Logging;
	using Sys.Workflow.Bpmn.Models;
	using Sys.Workflow.Engine.Delegate.Events;
	using Sys.Workflow.Engine.Delegate.Events.Impl;
	using Sys.Workflow.Engine.Impl.Cmd;
	using Sys.Workflow.Engine.Impl.Interceptor;
	using Sys.Workflow.Engine.Impl.Persistence.Entity;
	using Sys.Workflow.Engine.Impl.Util;
	using Sys.Workflow;

	public class TimerStartEventJobHandler : TimerEventHandler, IJobHandler
	{
		public const string TYPE = "timer-start-event";

		private readonly ILogger log = ProcessEngineServiceProvider.LoggerService<TimerStartEventJobHandler>();

		public virtual string Type
		{
			get
			{
				return TYPE;
			}
		}

		public virtual void Execute(IJobEntity job, string configuration, IExecutionEntity execution, ICommandContext commandContext)
		{
			IProcessDefinitionEntity processDefinitionEntity = ProcessDefinitionUtil.GetProcessDefinitionFromDatabase(job.ProcessDefinitionId); // From DB -> need to get latest suspended state
			if (processDefinitionEntity is null)
			{
				throw new ActivitiException("Could not find process definition needed for timer start event");
			}

			try
			{
				if (!processDefinitionEntity.Suspended)
				{
					if (commandContext.EventDispatcher.Enabled)
					{
						commandContext.EventDispatcher.DispatchEvent(ActivitiEventBuilder.CreateEntityEvent(ActivitiEventType.TIMER_FIRED, job));
					}

					// Find initial flow element matching the signal start event
					Process process = ProcessDefinitionUtil.GetProcess(job.ProcessDefinitionId);
					string activityId = GetActivityIdFromConfiguration(configuration);
					if (string.IsNullOrWhiteSpace(activityId) == false)
					{
						FlowElement flowElement = process.GetFlowElement(activityId, true);
						if (flowElement is null)
						{
							throw new ActivitiException("Could not find matching FlowElement for activityId " + activityId);
						}
						var processInstanceHelper = commandContext.ProcessEngineConfiguration.ProcessInstanceHelper;
						processInstanceHelper.CreateAndStartProcessInstanceWithInitialFlowElement(processDefinitionEntity, null, null, flowElement, process, null, null, true);
					}
					else
					{
						(new StartProcessInstanceCmd(processDefinitionEntity.Key, null, null, null, job.TenantId)).Execute(commandContext);
					}
				}
				else
				{
					log.LogDebug($"ignoring timer of suspended process definition {processDefinitionEntity.Name}");
				}
			}
			catch (Exception e)
			{
				log.LogError($"exception during timer execution: {e.Message}");

				throw new ActivitiException($"exception during timer execution: {e.Message}", e);
			}
		}
	}
}