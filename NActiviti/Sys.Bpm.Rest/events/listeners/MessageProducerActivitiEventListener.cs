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
    using org.activiti.cloud.services.api.events;
    using org.activiti.cloud.services.events.converter;
    using org.activiti.engine.@delegate.@event;

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @Component public class MessageProducerActivitiEventListener implements org.activiti.engine.delegate.event.ActivitiEventListener
    public class MessageProducerActivitiEventListener : IActivitiEventListener
    {

        private readonly EventConverterContext converterContext;

        private readonly ProcessEngineEventsAggregator eventsAggregator;

        //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        //ORIGINAL LINE: @Autowired public MessageProducerActivitiEventListener(org.activiti.cloud.services.events.converter.EventConverterContext converterContext, ProcessEngineEventsAggregator eventsAggregator)
        public MessageProducerActivitiEventListener(EventConverterContext converterContext, ProcessEngineEventsAggregator eventsAggregator)
        {
            this.converterContext = converterContext;
            this.eventsAggregator = eventsAggregator;
        }

        public virtual void onEvent(IActivitiEvent @event)
        {
            ProcessEngineEvent newEvent = converterContext.from(@event);
            if (newEvent != null)
            {
                eventsAggregator.add(newEvent);
            }
        }

        public virtual bool FailOnException
        {
            get
            {
                return false;
            }
        }
    }

}