/*
 * Copyright 2018 Alfresco and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace Sys.Workflow.Cloud.Services.Events
{
    using Sys.Workflow.Cloud.Services.Api.Events;

    /// <summary>
    /// 
    /// </summary>
    public interface IActivityStartedEvent : IProcessEngineEvent
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns> the id of the activity this event is related to. This corresponds to an id defined in the process definition. </returns>
        string ActivityId { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> the name of the activity this event is related to. </returns>
        string ActivityName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns> the type of the activity (if set during parsing). </returns>
        string ActivityType { get; }

    }

}