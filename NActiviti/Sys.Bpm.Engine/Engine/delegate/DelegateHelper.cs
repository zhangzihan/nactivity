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
namespace Sys.Workflow.Engine.Delegate
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.EL;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Impl.Util;

    /// <summary>
    /// Class that provides helper operations for use in the <seealso cref="ICSharpDelegate"/>,
    /// <seealso cref="ActivityBehavior"/>, <seealso cref="IExecutionListener"/> and <seealso cref="ITaskListener"/>
    /// interfaces.
    /// </summary>
    public class DelegateHelper
    {

        /// <summary>
        /// To be used in an <seealso cref="ActivityBehavior"/> or <seealso cref="ICSharpDelegate"/>: leaves
        /// according to the default BPMN 2.0 rules: all sequenceflow with a condition
        /// that evaluates to true are followed.
        /// </summary>
        public static void LeaveDelegate(IExecutionEntity delegateExecution)
        {
            Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(delegateExecution, true);
        }

        /// <summary>
        /// To be used in an <seealso cref="ActivityBehavior"/> or <seealso cref="ICSharpDelegate"/>: leaves
        /// the current activity via one specific sequenceflow.
        /// </summary>
        public static void LeaveDelegate(IExecutionEntity delegateExecution, string sequenceFlowId)
        {
            string processDefinitionId = delegateExecution.ProcessDefinitionId;
            Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);
            FlowElement flowElement = process.FindFlowElement(sequenceFlowId);
            if (flowElement is SequenceFlow)
            {
                delegateExecution.CurrentFlowElement = flowElement;
                Context.Agenda.PlanTakeOutgoingSequenceFlowsOperation(delegateExecution, false);
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
        public static BpmnModel GetBpmnModel(IExecutionEntity execution)
        {
            if (execution is null)
            {
                throw new ActivitiException("Null execution passed");
            }
            return ProcessDefinitionUtil.GetBpmnModel(execution.ProcessDefinitionId);
        }

        /// <summary>
        /// Returns the current <seealso cref="FlowElement"/> where the <seealso cref="IDelegateExecution"/> is currently at.
        /// </summary>
        public static FlowElement GetFlowElement(IExecutionEntity execution)
        {
            BpmnModel bpmnModel = GetBpmnModel(execution);
            FlowElement flowElement = bpmnModel.GetFlowElement(execution.CurrentActivityId);
            if (flowElement is null)
            {
                throw new ActivitiException("Could not find a FlowElement for activityId " + execution.CurrentActivityId);
            }
            return flowElement;
        }

        /// <summary>
        /// Returns whether or not the provided execution is being use for executing an <seealso cref="IExecutionListener"/>.
        /// </summary>
        public static bool IsExecutingExecutionListener(IExecutionEntity execution)
        {
            return execution.CurrentActivitiListener is object;
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
        public static IDictionary<string, IList<ExtensionElement>> GetExtensionElements(IExecutionEntity execution)
        {
            if (IsExecutingExecutionListener(execution))
            {
                return GetListenerExtensionElements(execution);
            }
            else
            {
                return GetFlowElementExtensionElements(execution);
            }
        }

        public static IDictionary<string, IList<ExtensionElement>> GetFlowElementExtensionElements(IExecutionEntity execution)
        {
            return GetFlowElement(execution).ExtensionElements;
        }

        public static IDictionary<string, IList<ExtensionElement>> GetListenerExtensionElements(IExecutionEntity execution)
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
        public static IList<FieldExtension> GetFields(IExecutionEntity execution)
        {
            if (IsExecutingExecutionListener(execution))
            {
                return GetListenerFields(execution);
            }
            else
            {
                return GetFlowElementFields(execution);
            }
        }

        public static IList<FieldExtension> GetFlowElementFields(IExecutionEntity execution)
        {
            FlowElement flowElement = GetFlowElement(execution);
            if (flowElement is TaskWithFieldExtensions)
            {
                return ((TaskWithFieldExtensions)flowElement).FieldExtensions;
            }
            return new List<FieldExtension>();
        }

        public static IList<FieldExtension> GetListenerFields(IExecutionEntity execution)
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
        public static FieldExtension GetField(IExecutionEntity execution, string fieldName)
        {
            if (IsExecutingExecutionListener(execution))
            {
                return GetListenerField(execution, fieldName);
            }
            else
            {
                return GetFlowElementField(execution, fieldName);
            }
        }

        public static FieldExtension GetFlowElementField(IExecutionEntity execution, string fieldName)
        {
            IList<FieldExtension> fieldExtensions = GetFlowElementFields(execution);
            if (fieldExtensions is null || fieldExtensions.Count == 0)
            {
                return null;
            }
            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                if (fieldExtension.FieldName is object && fieldExtension.FieldName.Equals(fieldName))
                {
                    return fieldExtension;
                }
            }
            return null;
        }

        public static FieldExtension GetListenerField(IExecutionEntity execution, string fieldName)
        {
            IList<FieldExtension> fieldExtensions = GetListenerFields(execution);
            if (fieldExtensions is null || fieldExtensions.Count == 0)
            {
                return null;
            }
            foreach (FieldExtension fieldExtension in fieldExtensions)
            {
                if (fieldExtension.FieldName is object && fieldExtension.FieldName.Equals(fieldName))
                {
                    return fieldExtension;
                }
            }
            return null;
        }

        /// <summary>
        /// Creates an <seealso cref="IExpression"/> for the <seealso cref="FieldExtension"/>.
        /// </summary>
        public static IExpression CreateExpressionForField(FieldExtension fieldExtension)
        {
            if (!string.IsNullOrWhiteSpace(fieldExtension.Expression))
            {
                ExpressionManager expressionManager = Context.ProcessEngineConfiguration.ExpressionManager;
                return expressionManager.CreateExpression(fieldExtension.Expression);
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
        public static IExpression GetFieldExpression(IExecutionEntity execution, string fieldName)
        {
            if (IsExecutingExecutionListener(execution))
            {
                return GetListenerFieldExpression(execution, fieldName);
            }
            else
            {
                return GetFlowElementFieldExpression(execution, fieldName);
            }
        }

        /// <summary>
        /// Similar to <seealso cref="#getFieldExpression(DelegateExecution, String)"/>, but for use within a <seealso cref="ITaskListener"/>.
        /// </summary>
        public static IExpression GetFieldExpression(IDelegateTask task, string fieldName)
        {
            if (task.CurrentActivitiListener is object)
            {
                IList<FieldExtension> fieldExtensions = task.CurrentActivitiListener.FieldExtensions;
                if (fieldExtensions is object && fieldExtensions.Count > 0)
                {
                    foreach (FieldExtension fieldExtension in fieldExtensions)
                    {
                        if (fieldName.Equals(fieldExtension.FieldName))
                        {
                            return CreateExpressionForField(fieldExtension);
                        }
                    }
                }
            }
            return null;
        }

        public static IExpression GetFlowElementFieldExpression(IExecutionEntity execution, string fieldName)
        {
            FieldExtension fieldExtension = GetFlowElementField(execution, fieldName);
            if (fieldExtension is object)
            {
                return CreateExpressionForField(fieldExtension);
            }
            return null;
        }

        public static IExpression GetListenerFieldExpression(IExecutionEntity execution, string fieldName)
        {
            FieldExtension fieldExtension = GetListenerField(execution, fieldName);
            if (fieldExtension is object)
            {
                return CreateExpressionForField(fieldExtension);
            }
            return null;
        }
    }

}