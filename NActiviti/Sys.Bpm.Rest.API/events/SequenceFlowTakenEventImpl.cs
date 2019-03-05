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

namespace org.activiti.cloud.services.events
{
    public class SequenceFlowTakenEventImpl : AbstractProcessEngineEvent, ISequenceFlowTakenEvent
    {
        private string sequenceFlowId;
        private string sourceActivityId;
        private string sourceActivityName;
        private string sourceActivityType;
        private string targetActivityId;
        private string targetActivityName;
        private string targetActivityType;

        public SequenceFlowTakenEventImpl(string appName, string appVersion, string serviceName, string serviceFullName, string serviceType, string serviceVersion, string executionId, string processDefinitionId, string processInstanceId, string sequenceFlowId, string sourceActivityId, string sourceActivityName, string sourceActivityType, string targetActivityId, string targetActivityName, string targetActivityType) : base(appName, appVersion, serviceName, serviceFullName, serviceType, serviceVersion, executionId, processDefinitionId, processInstanceId)
        {
            this.sequenceFlowId = sequenceFlowId;
            this.sourceActivityId = sourceActivityId;
            this.sourceActivityName = sourceActivityName;
            this.sourceActivityType = sourceActivityType;
            this.targetActivityId = targetActivityId;
            this.targetActivityName = targetActivityName;
            this.targetActivityType = targetActivityType;
        }

        public override string EventType
        {
            get
            {
                return "SequenceFlowTakenEvent";
            }
        }

        public virtual string SequenceFlowId
        {
            get
            {
                return sequenceFlowId;
            }
        }

        public virtual string SourceActivityId
        {
            get
            {
                return sourceActivityId;
            }
        }

        public virtual string SourceActivityName
        {
            get
            {
                return sourceActivityName;
            }
        }

        public virtual string SourceActivityType
        {
            get
            {
                return sourceActivityType;
            }
        }

        public virtual string TargetActivityId
        {
            get
            {
                return targetActivityId;
            }
        }

        public virtual string TargetActivityName
        {
            get
            {
                return targetActivityName;
            }
        }

        public virtual string TargetActivityType
        {
            get
            {
                return targetActivityType;
            }
        }
    }

}