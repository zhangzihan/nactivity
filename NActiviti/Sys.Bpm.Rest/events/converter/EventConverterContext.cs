using Microsoft.Extensions.Logging;
using Sys.Workflow.cloud.services.api.events;
using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.engine.@delegate.@event;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow.engine.task;
using Sys.Workflow;
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

namespace Sys.Workflow.cloud.services.events.converter
{
    /// <summary>
    /// 
    /// </summary>
    public class EventConverterContext
    {
        private readonly ILogger logger = null;

        /// <summary>
        /// ProcessInstance:
        /// </summary>
        public const string PROCESS_EVENT_PREFIX = "ProcessInstance:";
        /// <summary>
        /// Task:
        /// </summary>
        public const string TASK_EVENT_PREFIX = "Task:";
        /// <summary>
        /// TaskCandidateUser:
        /// </summary>
        public const string TASK_CANDIDATE_USER_EVENT_PREFIX = "TaskCandidateUser:";
        /// <summary>
        /// TaskCandidateGroup:
        /// </summary>
        public const string TASK_CANDIDATE_GROUP_EVENT_PREFIX = "TaskCandidateGroup:";
        /// <summary>
        /// ""
        /// </summary>
        public const string EVENT_PREFIX = "";

        private readonly IDictionary<string, IEventConverter> convertersMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="convertersMap"></param>
        public EventConverterContext(IDictionary<string, IEventConverter> convertersMap)
        {
            this.convertersMap = convertersMap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="converters"></param>
        /// <param name="loggerFactory"></param>
        public EventConverterContext(ISet<IEventConverter> converters,
            ILoggerFactory loggerFactory)
        {
            this.convertersMap = converters.ToDictionary(x => x.GetType().FullName);
            logger = loggerFactory.CreateLogger<EventConverterContext>();
        }

        /// <summary>
        /// 
        /// </summary>
        internal virtual IDictionary<string, IEventConverter> ConvertersMap
        {
            get
            {
                return convertersMap;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        public virtual IProcessEngineEvent From(IActivitiEvent activitiEvent)
        {
            IEventConverter converter = convertersMap[GetPrefix(activitiEvent) + activitiEvent.Type];

            IProcessEngineEvent newEvent = null;
            if (converter != null)
            {
                newEvent = converter.From(activitiEvent);
            }
            else
            {
                logger.LogDebug(">> Ommited Event Type: " + activitiEvent.GetType().FullName);
            }
            return newEvent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        public static string GetPrefix(IActivitiEvent activitiEvent)
        {
            if (IsProcessEvent(activitiEvent))
            {
                return PROCESS_EVENT_PREFIX;
            }
            else if (IsTaskEvent(activitiEvent))
            {
                return TASK_EVENT_PREFIX;
            }
            else if (IsIdentityLinkEntityEvent(activitiEvent))
            {
                IIdentityLink identityLinkEntity = (IIdentityLink)((IActivitiEntityEvent)activitiEvent).Entity;
                if (IsCandidateUserEntity(identityLinkEntity))
                {
                    return TASK_CANDIDATE_USER_EVENT_PREFIX;
                }
                else if (IsCandidateGroupEntity(identityLinkEntity))
                {
                    return TASK_CANDIDATE_GROUP_EVENT_PREFIX;
                }
            }

            return EVENT_PREFIX;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        private static bool IsProcessEvent(IActivitiEvent activitiEvent)
        {
            bool isProcessEvent = false;
            if (activitiEvent is IActivitiEntityEvent)
            {
                object entity = ((IActivitiEntityEvent)activitiEvent).Entity;
                if (entity != null && entity.GetType().IsAssignableFrom(typeof(ProcessInstance)))
                {
                    isProcessEvent = !IsExecutionEntityEvent(activitiEvent) || ((IExecutionEntity)entity).ProcessInstanceType;
                }
            }
            else if (activitiEvent.Type == ActivitiEventType.PROCESS_CANCELLED)
            {
                isProcessEvent = true;
            }

            return isProcessEvent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        private static bool IsExecutionEntityEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent.Type == ActivitiEventType.ENTITY_SUSPENDED || activitiEvent.Type == ActivitiEventType.ENTITY_ACTIVATED || activitiEvent.Type == ActivitiEventType.ENTITY_CREATED;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        private static bool IsTaskEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent is IActivitiEntityEvent && ((IActivitiEntityEvent)activitiEvent).Entity is TaskModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activitiEvent"></param>
        /// <returns></returns>
        private static bool IsIdentityLinkEntityEvent(IActivitiEvent activitiEvent)
        {
            return activitiEvent is IActivitiEntityEvent && ((IActivitiEntityEvent)activitiEvent).Entity is IIdentityLink;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identityLinkEntity"></param>
        /// <returns></returns>
        private static bool IsCandidateUserEntity(IIdentityLink identityLinkEntity)
        {
            return string.Compare(IdentityLinkType.CANDIDATE, identityLinkEntity.Type, true) == 0 && identityLinkEntity.UserId != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identityLinkEntity"></param>
        /// <returns></returns>
        private static bool IsCandidateGroupEntity(IIdentityLink identityLinkEntity)
        {
            return string.Compare(IdentityLinkType.CANDIDATE, identityLinkEntity.Type, true) == 0 && identityLinkEntity.GroupId != null;
        }
    }

}