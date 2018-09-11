using Microsoft.Extensions.Logging;
using org.activiti.cloud.services.api.events;
using org.activiti.cloud.services.api.model;
using org.activiti.engine.@delegate.@event;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.task;
using Sys;
using System;
using System.Collections.Generic;
using System.Linq;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.cloud.services.events.converter
{
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.activiti.engine.@delegate.@event.ActivitiEventType.ENTITY_ACTIVATED;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.activiti.engine.@delegate.@event.ActivitiEventType.ENTITY_CREATED;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.activiti.engine.@delegate.@event.ActivitiEventType.ENTITY_SUSPENDED;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.activiti.engine.@delegate.@event.ActivitiEventType.PROCESS_CANCELLED;
    //JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
    //	import static org.activiti.engine.task.IdentityLinkType.CANDIDATE;

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class EventConverterContext
    public class EventConverterContext
    {
        private static readonly ILogger LOGGER = ProcessEngineServiceProvider.LoggerService<EventConverterContext>();

        public const string PROCESS_EVENT_PREFIX = "ProcessInstance:";
        public const string TASK_EVENT_PREFIX = "Task:";
        public const string TASK_CANDIDATE_USER_EVENT_PREFIX = "TaskCandidateUser:";
        public const string TASK_CANDIDATE_GROUP_EVENT_PREFIX = "TaskCandidateGroup:";
        public const string EVENT_PREFIX = "";

        private IDictionary<string, EventConverter> convertersMap;

        public EventConverterContext(IDictionary<string, EventConverter> convertersMap)
        {
            this.convertersMap = convertersMap;
        }

        public EventConverterContext(ISet<EventConverter> converters)
        {
            this.convertersMap = converters.ToDictionary(x => x.GetType().FullName);
        }

        internal virtual IDictionary<string, EventConverter> ConvertersMap
        {
            get
            {
                return convertersMap;
            }
        }

        public virtual ProcessEngineEvent from(IActivitiEvent activitiEvent)
        {
            EventConverter converter = convertersMap[getPrefix(activitiEvent) + activitiEvent.Type];

            ProcessEngineEvent newEvent = null;
            if (converter != null)
            {
                newEvent = converter.from(activitiEvent);
            }
            else
            {
                //JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getCanonicalName method:
                LOGGER.LogDebug(">> Ommited Event Type: " + activitiEvent.GetType().FullName);
            }
            return newEvent;
        }

        public static string getPrefix(IActivitiEvent activitiEvent)
        {
            if (isProcessEvent(activitiEvent))
            {
                return PROCESS_EVENT_PREFIX;
            }
            else if (isTaskEvent(activitiEvent))
            {
                return TASK_EVENT_PREFIX;
            }
            else if (isIdentityLinkEntityEvent(activitiEvent))
            {
                IIdentityLink identityLinkEntity = (IIdentityLink)((IActivitiEntityEvent)activitiEvent).Entity;
                if (isCandidateUserEntity(identityLinkEntity))
                {
                    return TASK_CANDIDATE_USER_EVENT_PREFIX;
                }
                else if (isCandidateGroupEntity(identityLinkEntity))
                {
                    return TASK_CANDIDATE_GROUP_EVENT_PREFIX;
                }
            }

            return EVENT_PREFIX;
        }

        private static bool isProcessEvent(IActivitiEvent activitiEvent)
        {
            bool isProcessEvent = false;
            if (activitiEvent is IActivitiEntityEvent)
            {
                object entity = ((IActivitiEntityEvent)activitiEvent).Entity;
                if (entity != null && entity.GetType().IsAssignableFrom(typeof(ProcessInstance)))
                {
                    isProcessEvent = !isExecutionEntityEvent(activitiEvent) || ((IExecutionEntity)entity).ProcessInstanceType;
                }
            }
            else if (activitiEvent.Type == ActivitiEventType.PROCESS_CANCELLED)
            {
                isProcessEvent = true;
            }

            return isProcessEvent;
        }

        private static bool isExecutionEntityEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent.Type == ActivitiEventType.ENTITY_SUSPENDED || activitiEvent.Type == ActivitiEventType.ENTITY_ACTIVATED || activitiEvent.Type == ActivitiEventType.ENTITY_CREATED;
        }

        private static bool isTaskEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent is IActivitiEntityEvent && ((IActivitiEntityEvent)activitiEvent).Entity is Task;
        }

        private static bool isIdentityLinkEntityEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent is IActivitiEntityEvent && ((IActivitiEntityEvent)activitiEvent).Entity is IIdentityLink;
        }

        private static bool isCandidateUserEntity(IIdentityLink identityLinkEntity)
        {
            return string.Compare(IdentityLinkType.CANDIDATE, identityLinkEntity.Type, true) == 0 && identityLinkEntity.UserId != null;
        }

        private static bool isCandidateGroupEntity(IIdentityLink identityLinkEntity)
        {
            return string.Compare(IdentityLinkType.CANDIDATE, identityLinkEntity.Type, true) == 0 && identityLinkEntity.GroupId != null;
        }
    }

}