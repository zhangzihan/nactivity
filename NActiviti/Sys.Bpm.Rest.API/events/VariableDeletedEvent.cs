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

namespace Sys.Workflow.cloud.services.events
{
    using IProcessEngineEvent = api.events.IProcessEngineEvent;


    /// <summary>
    /// 
    /// </summary>
    public interface IVariableDeletedEvent : IProcessEngineEvent
    {


        /// <summary>
        /// 
        /// </summary>
        /// <returns> the name of the variable involved. </returns>
        string VariableName { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns> The variable type as string. </returns>
        string VariableType { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns> the id of the execution the variable is set on. </returns>
        new string ExecutionId { get; }


        /// <summary>
        /// 
        /// </summary>
        /// <returns> the id of the task the variable has been set on. </returns>
        string TaskId { get; }
    }

}