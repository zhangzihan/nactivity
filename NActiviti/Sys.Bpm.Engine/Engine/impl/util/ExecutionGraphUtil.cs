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
namespace Sys.Workflow.Engine.Impl.Util
{

    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    public class ExecutionGraphUtil
    {

        /// <summary>
        /// Takes in a collection of executions belonging to the same process instance. Orders the executions in a list, first elements are the leaf, last element is the root elements.
        /// </summary>
        public static IList<IExecutionEntity> OrderFromRootToLeaf(ICollection<IExecutionEntity> executions)
        {
            IList<IExecutionEntity> orderedList = new List<IExecutionEntity>(executions.Count);

            // Root elements
            HashSet<string> previousIds = new HashSet<string>();
            foreach (IExecutionEntity execution in executions)
            {
                if (execution.ParentId is null)
                {
                    orderedList.Add(execution);
                    previousIds.Add(execution.Id);
                }
            }

            // Non-root elements
            while (orderedList.Count < executions.Count)
            {
                foreach (IExecutionEntity execution in executions)
                {
                    if (!previousIds.Contains(execution.Id) && previousIds.Contains(execution.ParentId))
                    {
                        orderedList.Add(execution);
                        previousIds.Add(execution.Id);
                    }
                }
            }

            return orderedList;
        }

        public static IList<IExecutionEntity> OrderFromLeafToRoot(ICollection<IExecutionEntity> executions)
        {
            IList<IExecutionEntity> orderedList = OrderFromRootToLeaf(executions);
            //orderedList.Reverse();
            return orderedList;
        }

        /// <summary>
        /// Verifies if the element with the given source identifier can reach the element with the target identifier through following sequence flow.
        /// </summary>
        public static bool IsReachable(string processDefinitionId, string sourceElementId, string targetElementId)
        {
            // Fetch source and target elements
            Process process = ProcessDefinitionUtil.GetProcess(processDefinitionId);

            FlowElement sourceFlowElement = process.GetFlowElement(sourceElementId, true);
            FlowNode sourceElement = null;
            if (sourceFlowElement is FlowNode)
            {
                sourceElement = (FlowNode)sourceFlowElement;
            }
            else if (sourceFlowElement is SequenceFlow)
            {
                sourceElement = (FlowNode)((SequenceFlow)sourceFlowElement).TargetFlowElement;
            }

            FlowElement targetFlowElement = process.GetFlowElement(targetElementId, true);
            FlowNode targetElement = null;
            if (targetFlowElement is FlowNode)
            {
                targetElement = (FlowNode)targetFlowElement;
            }
            else if (targetFlowElement is SequenceFlow)
            {
                targetElement = (FlowNode)((SequenceFlow)targetFlowElement).TargetFlowElement;
            }

            if (sourceElement is null)
            {
                throw new ActivitiException("Invalid sourceElementId '" + sourceElementId + "': no element found for this id n process definition '" + processDefinitionId + "'");
            }
            if (targetElement is null)
            {
                throw new ActivitiException("Invalid targetElementId '" + targetElementId + "': no element found for this id n process definition '" + processDefinitionId + "'");
            }

            ISet<string> visitedElements = new HashSet<string>();
            return IsReachable(process, sourceElement, targetElement, visitedElements);
        }

        public static bool IsReachable(Process process, FlowNode sourceElement, FlowNode targetElement, ISet<string> visitedElements)
        {

            // No outgoing seq flow: could be the end of eg . the process or an embedded subprocess
            if (sourceElement.OutgoingFlows.Count == 0)
            {
                visitedElements.Add(sourceElement.Id);

                IFlowElementsContainer parentElement = process.FindParent(sourceElement);
                if (parentElement is SubProcess)
                {
                    sourceElement = (SubProcess)parentElement;
                }
                else
                {
                    return false;
                }
            }

            if (sourceElement.Id.Equals(targetElement.Id))
            {
                return true;
            }

            // To avoid infinite looping, we must capture every node we visit
            // and check before going further in the graph if we have already
            // visited the node.
            visitedElements.Add(sourceElement.Id);

            IList<SequenceFlow> sequenceFlows = sourceElement.OutgoingFlows;
            if (sequenceFlows is object && sequenceFlows.Count > 0)
            {
                foreach (SequenceFlow sequenceFlow in sequenceFlows)
                {
                    string targetRef = sequenceFlow.TargetRef;
                    FlowNode sequenceFlowTarget = (FlowNode)process.GetFlowElement(targetRef, true);
                    if (sequenceFlowTarget is object && !visitedElements.Contains(sequenceFlowTarget.Id))
                    {
                        bool reachable = IsReachable(process, sequenceFlowTarget, targetElement, visitedElements);

                        if (reachable)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

    }

}