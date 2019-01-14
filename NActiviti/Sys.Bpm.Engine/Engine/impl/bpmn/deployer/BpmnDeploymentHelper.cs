using System.Collections.Generic;

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
namespace org.activiti.engine.impl.bpmn.deployer
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.interceptor;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;

    /// <summary>
    /// Methods for working with deployments.  Much of the actual work of <seealso cref="BpmnDeployer"/> is
    /// done by orchestrating the different pieces of work this class does; by having them here,
    /// we allow other deployers to make use of them.   
    /// </summary>
    public class BpmnDeploymentHelper
    {

        protected internal TimerManager timerManager;
        protected internal EventSubscriptionManager eventSubscriptionManager;

        /// <summary>
        /// Verifies that no two process definitions share the same key, to prevent database unique
        /// index violation.
        /// </summary>
        /// <exception cref="ActivitiException"> if any two processes have the same key </exception>
        public virtual void verifyProcessDefinitionsDoNotShareKeys(ICollection<IProcessDefinitionEntity> processDefinitions)
        {
            IList<string> keySet = new List<string>();
            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {
                if (keySet.Contains(processDefinition.Key))
                {
                    throw new ActivitiException("The deployment contains process definitions with the same key (process id attribute), this is not allowed");
                }
                keySet.Add(processDefinition.Key);
            }
        }

        /// <summary>
        /// Updates all the process definition entities to match the deployment's values for tenant,
        /// engine version, and deployment id.
        /// </summary>
        public virtual void copyDeploymentValuesToProcessDefinitions(IDeploymentEntity deployment, IList<IProcessDefinitionEntity> processDefinitions)
        {
            string engineVersion = deployment.EngineVersion;
            string tenantId = deployment.TenantId;
            string deploymentId = deployment.Id;

            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {

                // Backwards compatibility
                if (!string.ReferenceEquals(engineVersion, null))
                {
                    processDefinition.EngineVersion = engineVersion;
                }

                // process definition inherits the tenant id
                if (!string.ReferenceEquals(tenantId, null))
                {
                    processDefinition.TenantId = tenantId;
                }

                processDefinition.DeploymentId = deploymentId;
            }
        }

        /// <summary>
        /// Updates all the process definition entities to have the correct resource names.
        /// </summary>
        public virtual ParsedDeployment ResourceNamesOnProcessDefinitions(ParsedDeployment value)
        {
            foreach (IProcessDefinitionEntity processDefinition in value.AllProcessDefinitions)
            {
                string resourceName = value.getResourceForProcessDefinition(processDefinition).Name;
                processDefinition.ResourceName = resourceName;
                processDefinition.Name = value.Deployment.Name;
            }

            return value;
        }

        /// <summary>
        /// Gets the most recent persisted process definition that matches this one for tenant and key.
        /// If none is found, returns null.  This method assumes that the tenant and key are properly
        /// set on the process definition entity.
        /// </summary>
        public virtual IProcessDefinitionEntity getMostRecentVersionOfProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string key = processDefinition.Key;
            string tenantId = processDefinition.TenantId;
            IProcessDefinitionEntityManager processDefinitionManager = Context.CommandContext.ProcessEngineConfiguration.ProcessDefinitionEntityManager;

            IProcessDefinitionEntity existingDefinition = null;

            if (!string.ReferenceEquals(tenantId, null) && !tenantId.Equals(ProcessEngineConfiguration.NO_TENANT_ID))
            {
                existingDefinition = processDefinitionManager.findLatestProcessDefinitionByKeyAndTenantId(key, tenantId);
            }
            else
            {
                existingDefinition = processDefinitionManager.findLatestProcessDefinitionByKey(key);
            }

            return existingDefinition;
        }

        /// <summary>
        /// Gets the persisted version of the already-deployed process definition.  Note that this is
        /// different from <seealso cref="#getMostRecentVersionOfProcessDefinition"/> as it looks specifically for
        /// a process definition that is already persisted and attached to a particular deployment,
        /// rather than the latest version across all deployments.
        /// </summary>
        public virtual IProcessDefinitionEntity getPersistedInstanceOfProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string deploymentId = processDefinition.DeploymentId;
            if (string.IsNullOrWhiteSpace(processDefinition.DeploymentId))
            {
                throw new System.InvalidOperationException("Provided process definition must have a deployment id.");
            }

            IProcessDefinitionEntityManager processDefinitionManager = Context.CommandContext.ProcessEngineConfiguration.ProcessDefinitionEntityManager;
            IProcessDefinitionEntity persistedProcessDefinition = null;
            if (string.ReferenceEquals(processDefinition.TenantId, null) || ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
            {
                persistedProcessDefinition = processDefinitionManager.findProcessDefinitionByDeploymentAndKey(deploymentId, processDefinition.Key);
            }
            else
            {
                persistedProcessDefinition = processDefinitionManager.findProcessDefinitionByDeploymentAndKeyAndTenantId(deploymentId, processDefinition.Key, processDefinition.TenantId);
            }

            return persistedProcessDefinition;
        }

        /// <summary>
        /// Updates all timers and events for the process definition.  This removes obsolete message and signal
        /// subscriptions and timers, and adds new ones.
        /// </summary>
        public virtual void updateTimersAndEvents(IProcessDefinitionEntity processDefinition, IProcessDefinitionEntity previousProcessDefinition, ParsedDeployment parsedDeployment)
        {

            Process process = parsedDeployment.getProcessModelForProcessDefinition(processDefinition);
            BpmnModel bpmnModel = parsedDeployment.getBpmnModelForProcessDefinition(processDefinition);

            eventSubscriptionManager.removeObsoleteMessageEventSubscriptions(previousProcessDefinition);
            eventSubscriptionManager.addMessageEventSubscriptions(processDefinition, process, bpmnModel);

            eventSubscriptionManager.removeObsoleteSignalEventSubScription(previousProcessDefinition);
            eventSubscriptionManager.addSignalEventSubscriptions(Context.CommandContext, processDefinition, process, bpmnModel);

            timerManager.removeObsoleteTimers(processDefinition);
            timerManager.scheduleTimers(processDefinition, process);
        }

        public enum ExpressionType
        {
            USER,
            GROUP
        }

        /// <param name="processDefinition"> </param>
        public virtual void addAuthorizationsForNewProcessDefinition(Process process, IProcessDefinitionEntity processDefinition)
        {
            ICommandContext commandContext = Context.CommandContext;

            addAuthorizationsFromIterator(commandContext, process.CandidateStarterUsers, processDefinition, ExpressionType.USER);
            addAuthorizationsFromIterator(commandContext, process.CandidateStarterGroups, processDefinition, ExpressionType.GROUP);
        }

        protected internal virtual void addAuthorizationsFromIterator(ICommandContext commandContext, IList<string> expressions, IProcessDefinitionEntity processDefinition, ExpressionType expressionType)
        {

            if (expressions != null)
            {
                IEnumerator<string> iterator = expressions.GetEnumerator();
                while (iterator.MoveNext())
                {
                    string expression = iterator.Current;
                    IIdentityLinkEntity identityLink = commandContext.IdentityLinkEntityManager.create();
                    identityLink.ProcessDef = processDefinition;
                    if (expressionType.Equals(ExpressionType.USER))
                    {
                        identityLink.UserId = expression;
                    }
                    else if (expressionType.Equals(ExpressionType.GROUP))
                    {
                        identityLink.GroupId = expression;
                    }
                    identityLink.Type = IdentityLinkType.CANDIDATE;
                    commandContext.IdentityLinkEntityManager.insert(identityLink);
                }
            }

        }

        public virtual TimerManager TimerManager
        {
            get
            {
                return timerManager;
            }
            set
            {
                this.timerManager = value;
            }
        }


        public virtual EventSubscriptionManager EventSubscriptionManager
        {
            get
            {
                return eventSubscriptionManager;
            }
            set
            {
                this.eventSubscriptionManager = value;
            }
        }

    }


}