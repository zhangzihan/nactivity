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
    /// <summary>
    /// 
    /// </summary>
    public class ActivityCompletedEventImpl : AbstractProcessEngineEvent, IActivityCompletedEvent
    {
        /// <summary>
        /// activityId
        /// </summary>
        protected internal string activityId;
        /// <summary>
        /// activityName
        /// </summary>
        protected internal string activityName;
        /// <summary>
        /// activityType
        /// </summary>
        protected internal string activityType;


        /// <summary>
        /// 
        /// </summary>
        public ActivityCompletedEventImpl()
        {
        }

        /// <summary>
        /// 
        /// </summary>

        public ActivityCompletedEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, string activityId, string activityName, string activityType) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.activityId = activityId;
            this.activityName = activityName;
            this.activityType = activityType;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string ActivityId
        {
            get
            {
                return activityId;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string ActivityName
        {
            get
            {
                return activityName;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual string ActivityType
        {
            get
            {
                return activityType;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public override string EventType
        {
            get
            {
                return "ActivityCompletedEvent";
            }
        }
    }

}