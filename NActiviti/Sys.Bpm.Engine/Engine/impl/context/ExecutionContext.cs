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

namespace Sys.Workflow.Engine.Impl.Contexts
{
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;
    using Sys.Workflow.Engine.Repository;
    using System.Collections.Generic;

    /// 
    public class ExecutionContext
    {
        private readonly IExecutionEntity execution;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        public ExecutionContext(IExecutionEntity execution)
        {
            this.execution = execution;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity Execution
        {
            get
            {
                return execution;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                return execution.ProcessInstance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IProcessDefinition ProcessDefinition
        {
            get
            {
                return ProcessDefinitionUtil.GetProcessDefinition(execution.ProcessDefinitionId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual IDeploymentEntity Deployment
        {
            get
            {
                string deploymentId = ProcessDefinition.DeploymentId;
                var ctx = Context.CommandContext;
                if (ctx is object)
                {
                    IDeploymentEntity deployment = ctx.DeploymentEntityManager.FindById<IDeploymentEntity>(deploymentId);
                    return deployment;
                }

                return null;
            }
        }
    }
}