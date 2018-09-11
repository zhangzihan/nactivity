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

namespace org.activiti.engine.impl.context
{
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;
    using org.activiti.engine.repository;
    using System.Collections.Generic;

    /// 
    public class ExecutionContext
    {

        protected internal IExecutionEntity execution;

        public ExecutionContext(IExecutionEntity execution)
        {
            this.execution = execution;
        }

        public virtual IExecutionEntity Execution
        {
            get
            {
                return execution;
            }
        }

        public virtual IExecutionEntity ProcessInstance
        {
            get
            {
                return execution.ProcessInstance;
            }
        }

        public virtual IProcessDefinition ProcessDefinition
        {
            get
            {
                return ProcessDefinitionUtil.getProcessDefinition(execution.ProcessDefinitionId);
            }
        }

        public virtual IDeploymentEntity Deployment
        {
            get
            {
                string deploymentId = ProcessDefinition.DeploymentId;
                var ctx = Context.CommandContext;
                if (ctx != null)
                {
                    IDeploymentEntity deployment = ctx.DeploymentEntityManager.findById<IDeploymentEntity>(new KeyValuePair<string, object>("id", deploymentId));
                    return deployment;
                }

                return null;
            }
        }
    }

}