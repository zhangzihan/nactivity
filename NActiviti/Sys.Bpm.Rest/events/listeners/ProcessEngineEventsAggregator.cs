﻿using Sys.Workflow.Cloud.Services.Api.Events;
using System;

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

namespace Sys.Workflow.Cloud.Services.Events.Listeners
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessEngineEventsAggregator : BaseCommandContextEventsAggregator<IProcessEngineEvent, MessageProducerCommandContextCloseListener>
    {

        private readonly MessageProducerCommandContextCloseListener closeListener;

        /// <summary>
        /// 
        /// </summary>
        public ProcessEngineEventsAggregator(MessageProducerCommandContextCloseListener closeListener)
        {
            this.closeListener = closeListener;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override Type CloseListenerClass
        {
            get
            {
                return typeof(MessageProducerCommandContextCloseListener);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override MessageProducerCommandContextCloseListener CloseListener
        {
            get
            {
                return closeListener;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override string AttributeKey
        {
            get
            {
                return MessageProducerCommandContextCloseListener.PROCESS_ENGINE_EVENTS;
            }
        }
    }
}