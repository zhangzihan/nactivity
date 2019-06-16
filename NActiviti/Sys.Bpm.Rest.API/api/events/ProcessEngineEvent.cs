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

namespace Sys.Workflow.cloud.services.api.events
{

    /// <summary>
    /// 
    /// </summary>
    public interface IProcessEngineEvent
    {

        /// <summary>
        /// 
        /// </summary>

        long? Timestamp { get; }


        /// <summary>
        /// 
        /// </summary>
        string EventType { get; }


        /// <summary>
        /// 
        /// </summary>
        string ExecutionId { get; }


        /// <summary>
        /// 
        /// </summary>
        string ProcessDefinitionId { get; }


        /// <summary>
        /// 
        /// </summary>
        string ProcessInstanceId { get; }


        /// <summary>
        /// 
        /// </summary>
        string AppName { get; }


        /// <summary>
        /// 
        /// </summary>
        string AppVersion { get; }


        /// <summary>
        /// 
        /// </summary>
        string ServiceName { get; }


        /// <summary>
        /// 
        /// </summary>
        string ServiceFullName { get; }


        /// <summary>
        /// 
        /// </summary>
        string ServiceType { get; }


        /// <summary>
        /// 
        /// </summary>
        string ServiceVersion { get; }
    }
}