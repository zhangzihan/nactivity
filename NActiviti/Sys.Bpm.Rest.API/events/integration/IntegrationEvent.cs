﻿/*
 * Copyright 2017 Alfresco, Inc. and/or its affiliates.
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

using Sys.Workflow.cloud.services.api.events;

namespace Sys.Workflow.cloud.services.events.integration
{
    /// <summary>
    /// 
    /// </summary>
    public interface IIntegrationEvent : IProcessEngineEvent
    {

        /// <summary>
        /// 
        /// </summary>
        string IntegrationContextId { get; }

        /// <summary>
        /// 
        /// </summary>
        string FlowNodeId { get; }

    }

}