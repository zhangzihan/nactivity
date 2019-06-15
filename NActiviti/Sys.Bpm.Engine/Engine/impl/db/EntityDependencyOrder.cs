using System;
using System.Collections.Generic;

namespace org.activiti.engine.impl.db
{

    using org.activiti.engine.impl.persistence.entity;
    using System.Linq;


    /// <summary>
    /// Maintains a list of all the entities in order of dependency.
    /// </summary>
    public class EntityDependencyOrder
	{
        /// <summary>
        /// 
        /// </summary>
		public static IList<Type> DELETE_ORDER = new List<Type>();

        /// <summary>
        /// 
        /// </summary>
        public static IList<Type> INSERT_ORDER = new List<Type>();

		static EntityDependencyOrder()
		{

			/*
			 * In the comments below:
			 * 
			 * 'FK to X' : X should be BELOW the entity
			 * 
			 * 'FK from X': X should be ABOVE the entity
			 * 
			 */

			/* No FK */
			DELETE_ORDER.Add(typeof(PropertyEntityImpl));

			/* No FK */
			DELETE_ORDER.Add(typeof(AttachmentEntityImpl));

			/* No FK */
			DELETE_ORDER.Add(typeof(CommentEntityImpl));

			/* No FK */
			DELETE_ORDER.Add(typeof(EventLogEntryEntityImpl));

			/*
			 * FK to Deployment
			 * FK to ByteArray 
			 */
			DELETE_ORDER.Add(typeof(ModelEntityImpl));

			/*
			 * FK to ByteArray
			 */
			DELETE_ORDER.Add(typeof(JobEntityImpl));
			DELETE_ORDER.Add(typeof(TimerJobEntityImpl));
			DELETE_ORDER.Add(typeof(SuspendedJobEntityImpl));
			DELETE_ORDER.Add(typeof(DeadLetterJobEntityImpl));

			/*
			 * FK to ByteArray
			 * FK to Exeution
			 */
			DELETE_ORDER.Add(typeof(VariableInstanceEntityImpl));

			/* 
			 * FK to ByteArray
			 * FK to ProcessDefinition
			 */
			DELETE_ORDER.Add(typeof(ProcessDefinitionInfoEntityImpl));

			/*
			 * FK from ModelEntity
			 * FK from JobEntity
			 * FK from VariableInstanceEntity
			 * 
			 * FK to DeploymentEntity
			 */
			DELETE_ORDER.Add(typeof(ByteArrayEntityImpl));

			/*
			 * FK from ModelEntity
			 * FK from JobEntity
			 * FK from VariableInstanceEntity
			 * 
			 * FK to DeploymentEntity
			 */
			DELETE_ORDER.Add(typeof(ResourceEntityImpl));

			/*
			 * FK from ByteArray
			 */
			DELETE_ORDER.Add(typeof(DeploymentEntityImpl));

			/*
			 * FK to Execution
			 */
			DELETE_ORDER.Add(typeof(EventSubscriptionEntityImpl));

			/*
			 * FK to Execution
			 */
			DELETE_ORDER.Add(typeof(CompensateEventSubscriptionEntityImpl));

			/*
			 * FK to Execution
			 */
			DELETE_ORDER.Add(typeof(MessageEventSubscriptionEntityImpl));

			/*
			 * FK to Execution
			 */
			DELETE_ORDER.Add(typeof(SignalEventSubscriptionEntityImpl));


			/*
			 * FK to process definition
			 * FK to Execution
			 * FK to Task
			 */
			DELETE_ORDER.Add(typeof(IdentityLinkEntityImpl));

			/*
			 * FK from IdentityLink
			 * 
			 * FK to Execution
			 * FK to process definition
			 */
			DELETE_ORDER.Add(typeof(TaskEntityImpl));

			/*
			 * FK from VariableInstance 
			 * FK from EventSubscription
			 * FK from IdentityLink
			 * FK from Task
			 * 
			 * FK to ProcessDefinition
			 */
			DELETE_ORDER.Add(typeof(ExecutionEntityImpl));

			/*
			 * FK from Task
			 * FK from IdentityLink
			 * FK from execution
			 */
			DELETE_ORDER.Add(typeof(ProcessDefinitionEntityImpl));



		  // History entities have no FK's

			DELETE_ORDER.Add(typeof(HistoricIdentityLinkEntityImpl));


			DELETE_ORDER.Add(typeof(HistoricActivityInstanceEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricProcessInstanceEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricTaskInstanceEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricScopeInstanceEntityImpl));

			DELETE_ORDER.Add(typeof(HistoricVariableInstanceEntityImpl));

			DELETE_ORDER.Add(typeof(HistoricDetailAssignmentEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricDetailTransitionInstanceEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricDetailVariableInstanceUpdateEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricFormPropertyEntityImpl));
			DELETE_ORDER.Add(typeof(HistoricDetailEntityImpl));

			INSERT_ORDER = new List<Type>(DELETE_ORDER.Reverse());
		}
	}

}