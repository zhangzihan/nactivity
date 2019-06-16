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
namespace Sys.Workflow.engine.impl.bpmn.deployer
{
    using Sys.Workflow.bpmn.constants;
    using Sys.Workflow.bpmn.model;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.interceptor;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.task;
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;

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
        public virtual void VerifyProcessDefinitionsDoNotShareKeys(ICollection<IProcessDefinitionEntity> processDefinitions)
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
        public virtual void CopyDeploymentValuesToProcessDefinitions(IDeploymentEntity deployment, IList<IProcessDefinitionEntity> processDefinitions)
        {
            string engineVersion = deployment.EngineVersion;
            string tenantId = deployment.TenantId;
            string deploymentId = deployment.Id;

            foreach (IProcessDefinitionEntity processDefinition in processDefinitions)
            {

                // Backwards compatibility
                if (!(engineVersion is null))
                {
                    processDefinition.EngineVersion = engineVersion;
                }

                // process definition inherits the tenant id
                if (!(tenantId is null))
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
                string resourceName = value.GetResourceForProcessDefinition(processDefinition).Name;
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
        public virtual IProcessDefinitionEntity GetMostRecentVersionOfProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string key = processDefinition.Key;
            string tenantId = processDefinition.TenantId;
            IProcessDefinitionEntityManager processDefinitionManager = Context.CommandContext.ProcessEngineConfiguration.ProcessDefinitionEntityManager;

            IProcessDefinitionEntity existingDefinition;
            if (!(tenantId is null) && !tenantId.Equals(ProcessEngineConfiguration.NO_TENANT_ID))
            {
                existingDefinition = processDefinitionManager.FindLatestProcessDefinitionByKeyAndTenantId(key, tenantId);
            }
            else
            {
                existingDefinition = processDefinitionManager.FindLatestProcessDefinitionByKey(key);
            }

            return existingDefinition;
        }

        /// <summary>
        /// Gets the persisted version of the already-deployed process definition.  Note that this is
        /// different from <seealso cref="#getMostRecentVersionOfProcessDefinition"/> as it looks specifically for
        /// a process definition that is already persisted and attached to a particular deployment,
        /// rather than the latest version across all deployments.
        /// </summary>
        public virtual IProcessDefinitionEntity GetPersistedInstanceOfProcessDefinition(IProcessDefinitionEntity processDefinition)
        {
            string deploymentId = processDefinition.DeploymentId;
            if (string.IsNullOrWhiteSpace(processDefinition.DeploymentId))
            {
                throw new InvalidOperationException("Provided process definition must have a deployment id.");
            }

            IProcessDefinitionEntityManager processDefinitionManager = Context.CommandContext.ProcessEngineConfiguration.ProcessDefinitionEntityManager;
            IProcessDefinitionEntity persistedProcessDefinition;
            if (processDefinition.TenantId is null || ProcessEngineConfiguration.NO_TENANT_ID.Equals(processDefinition.TenantId))
            {
                persistedProcessDefinition = processDefinitionManager.FindProcessDefinitionByDeploymentAndKey(deploymentId, processDefinition.Key);
            }
            else
            {
                persistedProcessDefinition = processDefinitionManager.FindProcessDefinitionByDeploymentAndKeyAndTenantId(deploymentId, processDefinition.Key, processDefinition.TenantId);
            }

            return persistedProcessDefinition;
        }

        /// <summary>
        /// Updates all timers and events for the process definition.  This removes obsolete message and signal
        /// subscriptions and timers, and adds new ones.
        /// </summary>
        public virtual void UpdateTimersAndEvents(IProcessDefinitionEntity processDefinition, IProcessDefinitionEntity previousProcessDefinition, ParsedDeployment parsedDeployment)
        {

            Process process = parsedDeployment.GetProcessModelForProcessDefinition(processDefinition);
            BpmnModel bpmnModel = parsedDeployment.GetBpmnModelForProcessDefinition(processDefinition);

            eventSubscriptionManager.RemoveObsoleteMessageEventSubscriptions(previousProcessDefinition);
            eventSubscriptionManager.AddMessageEventSubscriptions(processDefinition, process, bpmnModel);

            eventSubscriptionManager.RemoveObsoleteSignalEventSubScription(previousProcessDefinition);
            eventSubscriptionManager.AddSignalEventSubscriptions(Context.CommandContext, processDefinition, process, bpmnModel);

            timerManager.RemoveObsoleteTimers(processDefinition);
            timerManager.ScheduleTimers(processDefinition, process);
        }

        public Tuple<bool, MemoryStream> AddCamundaNamespace(MemoryStream ms)
        {
            XDocument doc = XDocument.Load(ms);

            XmlNameTable nameTable = doc.CreateReader(ReaderOptions.OmitDuplicateNamespaces).NameTable;
            var namespaceManager = new XmlNamespaceManager(nameTable);
            namespaceManager.AddNamespace(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE);

            bool changed = false;
            var attr = doc.Root.Attribute(XName.Get(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.XMLNS_NAMESPACE));
            if (attr == null)
            {
                doc.Root.Add(new XAttribute(XName.Get(BpmnXMLConstants.ACTIVITI_EXTENSIONS_PREFIX, BpmnXMLConstants.XMLNS_NAMESPACE), BpmnXMLConstants.ACTIVITI_EXTENSIONS_NAMESPACE));
                changed = true;
            }

            MemoryStream save = null;
            if (changed)
            {
                save = new MemoryStream();
                doc.Save(save);
                save.Seek(0, SeekOrigin.Begin);
            }

            return new Tuple<bool, MemoryStream>(changed, save);
        }

        public enum ExpressionType
        {
            USER,
            GROUP
        }

        /// <param name="processDefinition"> </param>
        public virtual void AddAuthorizationsForNewProcessDefinition(Process process, IProcessDefinitionEntity processDefinition)
        {
            ICommandContext commandContext = Context.CommandContext;

            AddAuthorizationsFromIterator(commandContext, process.CandidateStarterUsers, processDefinition, ExpressionType.USER);
            AddAuthorizationsFromIterator(commandContext, process.CandidateStarterGroups, processDefinition, ExpressionType.GROUP);
        }

        protected internal virtual void AddAuthorizationsFromIterator(ICommandContext commandContext, IList<string> expressions, IProcessDefinitionEntity processDefinition, ExpressionType expressionType)
        {
            if (expressions != null)
            {
                foreach (string expression in expressions)
                {
                    IIdentityLinkEntity identityLink = commandContext.IdentityLinkEntityManager.Create();
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
                    commandContext.IdentityLinkEntityManager.Insert(identityLink);
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