using Sys.Workflow.cloud.services.api.events;
using Sys.Workflow.engine.impl.interceptor;
using org.springframework.messaging.support;
using System.Collections.Generic;

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

namespace Sys.Workflow.cloud.services.events.listeners
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageProducerCommandContextCloseListener : ICommandContextCloseListener
    {
        /// <summary>
        /// processEngineEvents
        /// </summary>
        public const string PROCESS_ENGINE_EVENTS = "processEngineEvents";

        private readonly IProcessEngineChannels producer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="producer"></param>
        public MessageProducerCommandContextCloseListener(IProcessEngineChannels producer)
        {
            this.producer = producer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void Closed(ICommandContext commandContext)
        {
            IList<IProcessEngineEvent> events = commandContext.GetGenericAttribute<IList<IProcessEngineEvent>>(PROCESS_ENGINE_EVENTS);
            if (events != null && events.Count > 0)
            {
                producer.AuditProducer().Send(MessageBuilder<IList<IProcessEngineEvent>>.WithPayload(events).Build());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void Closing(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void AfterSessionsFlush(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandContext"></param>
        public virtual void CloseFailure(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }
    }
}