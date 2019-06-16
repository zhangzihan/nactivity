using System;

/* Licensed under the Apache License, Version 2.0 (the "License");
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
 */

namespace Sys.Workflow.engine.impl.cmd
{

    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.repository;
    using System.Collections.Generic;

    /// 
    [Serializable]
    public class GetDraftDeploymentCmd : ICommand<IList<IDeployment>>
    {
        private const long serialVersionUID = 1L;
        private readonly IDeploymentQuery query;

        public GetDraftDeploymentCmd(IDeploymentQuery query)
        {
            this.query = query;
        }

        public  virtual IList<IDeployment> Execute(ICommandContext  commandContext)
        {
            return commandContext.ProcessEngineConfiguration.DeploymentEntityManager.FindDrafts(this.query);
        }
    }

}