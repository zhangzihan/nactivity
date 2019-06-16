using System;
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

namespace Sys.Workflow.engine.impl.persistence.entity.integration
{

    public class IntegrationContextEntityImpl : AbstractEntity, IIntegrationContextEntity
    {

        private string executionId;

        private DateTime createdDate;

        private string processInstanceId;

        private string processDefinitionId;

        private string flowNodeId;

        public virtual string ExecutionId
        {
            get
            {
                return executionId;
            }
            set
            {
                this.executionId = value;
            }
        }


        public virtual string ProcessDefinitionId
        {
            get
            {
                return processDefinitionId;
            }
            set
            {
                this.processDefinitionId = value;
            }
        }


        public virtual string ProcessInstanceId
        {
            get
            {
                return processInstanceId;
            }
            set
            {
                this.processInstanceId = value;
            }
        }


        public virtual string FlowNodeId
        {
            get
            {
                return flowNodeId;
            }
            set
            {
                this.flowNodeId = value;
            }
        }


        public virtual DateTime CreatedDate
        {
            get
            {
                return createdDate;
            }
            set
            {
                this.createdDate = value;
            }
        }


        public override PersistentState PersistentState
        {
            get
            {
                return new PersistentState();
            }
        }
    }

}