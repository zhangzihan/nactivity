﻿using System.Collections.Generic;

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

namespace Sys.Workflow.engine.impl.cmd.integration
{
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity.integration;

    public class RetrieveIntegrationContextsCmd : ICommand<IList<IIntegrationContextEntity>>
    {

        private string executionId;

        public RetrieveIntegrationContextsCmd(string executionId)
        {
            this.executionId = executionId;
        }

        public virtual IList<IIntegrationContextEntity> Execute(ICommandContext commandContext)
        {
            return commandContext.ProcessEngineConfiguration.IntegrationContextManager.FindIntegrationContextByExecutionId(executionId);
        }
    }

}