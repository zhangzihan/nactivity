using Newtonsoft.Json;
using System;

/*
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

namespace org.activiti.cloud.services.api.model
{
    public class ProcessInstance
    {
        public enum ProcessInstanceStatus
        {
            RUNNING,
            SUSPENDED,
            COMPLETED
        }

        public ProcessInstance()
        {
        }

        public virtual string Id
        {
            get;set;
        }

        public virtual string Name
        {
            get;set;
        }

        public virtual string Description
        {
            get;set;
        }

        public virtual DateTime? StartDate
        {
            get;set;
        }

        public virtual string Initiator
        {
            get;set;
        }

        public virtual string BusinessKey
        {
            get;set;
        }

        public virtual string Status
        {
            get;set;
        }

        public virtual string ProcessDefinitionId
        {
            get;set;
        }

        public virtual string ProcessDefinitionKey
        {
            get;set;
        }
    }
}