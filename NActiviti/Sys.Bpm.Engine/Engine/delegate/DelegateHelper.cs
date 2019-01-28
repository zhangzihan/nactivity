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
namespace org.activiti.engine.@delegate
{

    using org.activiti.bpmn.model;
    using org.activiti.engine.impl.context;
    using org.activiti.engine.impl.el;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.impl.util;

    /// <summary>
    /// Class that provides helper operations for use in the <seealso cref="IJavaDelegate"/>,
    /// <seealso cref="ActivityBehavior"/>, <seealso cref="IExecutionListener"/> and <seealso cref="ITaskListener"/>
    /// interfaces.
    /// </summary>
    public class DelegateHelper
    {

        /// <summary>
        /// To be used in an <seealso cref="ActivityBehavior"/> or <seealso cref="IJavaDelegate"/>: leaves
        /// according to the default BPMN 2.0 rules: all sequenceflow with a condition
        /// that evaluates to true are followed.
        /// </summary>
        public static void leaveDelegate(IExecutionEntity delegateExecution)
        {
            Context.Agenda.planTakeOutgoingSequenceFlowsOperation(delegateExecution, true);
        }

        /// <summary>
        /// To be used in an <seealso cref="ActivityBehavior"/> or <seealso cref="IJavaDelegate"/>: leaves
        /// the current activity via one specific sequenceflow.
        /// </summary>
        public static void leaveDelegate(IExecutionEntity delegateExecution, string sequenceFlowId)
        {
            string processDefinitionId = delegateExecution.ProcessDefinitionId;
            Process process = ProcessDefinitionUtil.getProcess(processDefinitionId);
            FlowElement flowElement = process.getFlowElement(sequenceFlowId);
            if (flowElement is SequenceFlow)
            {
                delegateExecution.CurrentFlowElement = flowElement;
                Context.Agenda.planTakeOutgoingSequenceFlowsOperation(delegateExecution, false);
            }
            else
            {
                throw new ActivitiException(sequenceFlowId + " does not match a sequence flow");
            }
        }

        /// <summary>
        /// Returns the <seealso cref="BpmnModel"/> matching the process definition bpmn model
        /// for the process definition of the passed <seealso cref="IDelegateExecution"/>.
        /// </summary>
        public static BpmnModel getBpmnModel(IExecutionEntity execution)
        {
            if (execution == null)
            {
                throw new ActivitiException("Null execution passed");
            }
            return ProcessDefinitionUtil.getBpmnModel(execution.ProcessDefinitionId);
        }

        /// <summary>
        /// Returns the current <seealso cref="FlowElement"/> where the <seealso cref="IDelegateExecution"/> is currently at.
        /// </summary>
        public static FlowElement getFlowElement(IExecutionEntity execution)
        {
            BpmnModel bpmnModel = getBpmnModel(execution);
            FlowElement flowElement = bpmnModel.getFlowElement(execution.CurrentActivityId);
            if (flowElement == null)
            {
                throw new ActivitiException("Could not find a FlowElement for activityId " + execution.CurrentActivityId);
            }
            return flowElement;
        }

        /// <summary>
        /// Returns whether or not the provided execution is being use for executing an <seealso cref="IExecutionListener"/>.
        /// </summary>
        public static bool isExecutingExecutionListener(IExecutionEntity execution)
        {
            return execution.CurrentActivitiListener != null;
        }

        /// <summary>
        /// Returns for the activityId of the passed <seealso cref="IDelegateExecution"/> the
        /// <seealso cref="Map"/> of <seealso cref="ExtensionElement"/> instances. These represent the
        /// extension elements defined in the BPMN 2.0 XML as part of that particular
        /// activity.
        /// <para>
        /// If the execution is currently being used for executing an
        /// <seealso cref="IExecutionListener"/>, the extension elements of the listener will be
        /// used. Use the <seealso cref="#getFlowElementExtensionElements(DelegateExecution)"/>
        /// or <seealso cref="#getListenerExtensionElements(DelegateExecution)"/> instead to
        /// specifically get the extension elements of either the flow element or the
        /// listener.
        /// </para>
        /// </summary>
        public static IDictionary<string, IList<ExtensionElement>> getExtensionElements(IExecutionEntity execution)
        {
            if (isExecutingExecutionListener(execution))
            {
                return getListenerExtensionElements(execution);
            }
            else
            {
                return getFlowElementExtensionElements(execution);
            }
        }

        public static IDictionary<string, IList<ExtensionElement>> getFlowElementExtensionElements(IExecutionEntity execution)
        {
            return getFlowElement(execution).ExtensionElements;
        }

        public static IDictionary<string, IList<ExtensionElement>> getListenerExtensionElements(IExecutionEntity execution)
        {
            return execution.CurrentActivitiListener.ExtensionElements;
        }

        /// <summary>
        /// Returns the list of field extensions, represented as instances of
        /// <seealso cref="FieldExtension"/>, for the current activity of the passed
        /// <seealso cref="IDelegateExecution"/>.
        /// <para>
        /// If the execution is currently being used for executing an
        /// <seealso cref="IExecutionListener"/>, the fields of the listener will be returned. Use
        /// <seealso cref="#getFlowElementFields(DelegateExecution)"/> or
        /// <seealso cref="#getListenerFields(DelegateExecution)"/> if needing the flow element
        /// of listener fields specifically.
        /// </para>
        /// </summary>
        public static IList<FieldExtension> getFields(IExecutionEntity execution)
        {
            if (isExecutingExecutionListener(execution))
            {
                return getListenerFields(execution);
            }
            else
            {
                return getFlowElementFields(execution);
            }
        }

        public static IList<FieldExtension> getFlowElementFields(IExecutionEntity execution)
        {
            FlowElement flowElement = getFlowElement(execution);
            if (flowElement is TaskWithFieldExtensions)
            {
                return ((TaskWithFieldExtensions)flowElement).FieldExtensions;
            }
            return new List<FieldExtension>();
        }

        public static IList<FieldExtension> getListenerFields(IExecutionEntity execution)
        {
            return execution.CurrentActivitiListener.FieldExtensions;
        }

        /// <summary>
        /// Returns the <seealso cref="FieldExtension"/> matching the provided 'fieldName' which
        /// is defined for the current activity of the provided
        /// <seealso cref="IDelegateExecution"/>.
        /// <para>
        /// Returns null if no such <seealso cref="FieldExtension"/> can be found.
        /// </para>
        /// <para>
        /// If the execution is currently being used for executing an
        /// <seealso cref="IExecutionListener"/>, the field of the listener will be returned. Use
        /// <seealso cref="#getFlowElementField(DelegateExecution, String)"/> or
        /// <seealso cref="#getListenerField(DelegateExecution, String)"/> for specifically
        /// getting the field from either the flow element or the listener.
        /// </para>
        /// </summary>
        public static FieldExtension getField(IExecutionEntity execution, string fieldName)
        {
            if (isExecutingExecutionListener(execution))
            {
                return getListenerField(execution, fieldName);
            }
            else
            {
                return getFlowElementField(execution, fieldName);
            }
        }

        public static FieldExtension getFlowElementField(IExecutionEntity execution, string fieldName)
        {
            IList<FieldExtension> fieldExtensions = getFlowElementFields(execution);
            if (fieldExtensions == null || fieldExtensions.Count == 0)
            {
                return null;
            }
            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                if (!ReferenceEquals(fieldExtension.FieldName, null) && fieldExtension.FieldName.Equals(fieldName))
                {
                    return fieldExtension;
                }
            }
            return null;
        }

        public static FieldExtension getListenerField(IExecutionEntity execution, string fieldName)
        {
            IList<FieldExtension> fieldExtensions = getListenerFields(execution);
            if (fieldExtensions == null || fieldExtensions.Count == 0)
            {
                return null;
            }
            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                if (!ReferenceEquals(fieldExtension.FieldName, null) && fieldExtension.FieldName.Equals(fieldName))
                {
                    return fieldExtension;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates an <seealso cref="IExpression"/> for the <seealso cref="FieldExtension"/>.
        /// </summary>
        public static IExpression createExpressionForField(FieldExtension fieldExtension)
        {
            if (!string.IsNullOrWhiteSpace(fieldExtension.Expression))
            {
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;
                return expressionManager.createExpression(fieldExtension.Expression);
            }
            else
            {
                return new FixedValue(fieldExtension.StringValue);
            }
        }

        /// <summary>
        /// Returns the <seealso cref="IExpression"/> for the field defined for the current
        /// activity of the provided <seealso cref="IDelegateExecution"/>.
        /// <para>
        /// Returns null if no such field was found in the process definition xml.
        /// </para>
        /// <para>
        /// If the execution is currently being used for executing an
        /// <seealso cref="IExecutionListener"/>, it will return the field expression for the
        /// listener. Use
        /// <seealso cref="#getFlowElementFieldExpression(DelegateExecution, String)"/> or
        /// <seealso cref="#getListenerFieldExpression(DelegateExecution, String)"/> for
        /// specifically getting the flow element or listener field expression.
        /// </para>
        /// </summary>
        public static IExpression getFieldExpression(IExecutionEntity execution, string fieldName)
        {
            if (isExecutingExecutionListener(execution))
            {
                return getListenerFieldExpression(execution, fieldName);
            }
            else
            {
                return getFlowElementFieldExpression(execution, fieldName);
            }
        }

        /// <summary>
        /// Similar to <seealso cref="#getFieldExpression(DelegateExecution, String)"/>, but for use within a <seealso cref="ITaskListener"/>.
        /// </summary>
        public static IExpression getFieldExpression(IDelegateTask task, string fieldName)
        {
            if (task.CurrentActivitiListener != null)
            {
                IList<FieldExtension> fieldExtensions = task.CurrentActivitiListener.FieldExtensions;
                if (fieldExtensions != null && fieldExtensions.Count > 0)
                {
                    foreach (FieldExtension fieldExtension in fieldExtensions)
                    {
                        if (fieldName.Equals(fieldExtension.FieldName))
                        {
                            return createExpressionForField(fieldExtension);
                        }
                    }
                }
            }
            return null;
        }

        public static IExpression getFlowElementFieldExpression(IExecutionEntity execution, string fieldName)
        {
            FieldExtension fieldExtension = getFlowElementField(execution, fieldName);
            if (fieldExtension != null)
            {
                return createExpressionForField(fieldExtension);
            }
            return null;
        }

        public static IExpression getListenerFieldExpression(IExecutionEntity execution, string fieldName)
        {
            FieldExtension fieldExtension = getListenerField(execution, fieldName);
            if (fieldExtension != null)
            {
                return createExpressionForField(fieldExtension);
            }
            return null;
        }
    }

}