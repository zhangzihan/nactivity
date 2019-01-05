using org.activiti.cloud.services.api.events;
using org.activiti.engine.impl.interceptor;
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

namespace org.activiti.cloud.services.events.listeners
{
    public class MessageProducerCommandContextCloseListener : ICommandContextCloseListener
    {

        public const string PROCESS_ENGINE_EVENTS = "processEngineEvents";

        private readonly ProcessEngineChannels producer;

        public MessageProducerCommandContextCloseListener(ProcessEngineChannels producer)
        {
            this.producer = producer;
        }

        public virtual void closed(ICommandContext commandContext)
        {
            IList<ProcessEngineEvent> events = commandContext.getGenericAttribute<IList<ProcessEngineEvent>>(PROCESS_ENGINE_EVENTS);
            if (events != null && events.Count > 0)
            {
                producer.auditProducer().send(MessageBuilder<IList<ProcessEngineEvent>>.withPayload(events).build());
            }
        }

        public virtual void closing(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }

        public virtual void afterSessionsFlush(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }

        public virtual void closeFailure(ICommandContext commandContext)
        {
            // No need to implement this method in this class
        }
    }

}