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
namespace Sys.Workflow.engine.debug
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.persistence.entity;
    using Sys.Workflow.engine.runtime;
    using System;

    /// 
    public class ExecutionTreeUtil
    {

        public static ExecutionTree BuildExecutionTree(IExecutionEntity executionEntity)
        {
            // Find highest parent
            IExecutionEntity parentExecution = executionEntity;
            while (!(parentExecution.ParentId is null) || !(((IExecution)parentExecution).SuperExecutionId is null))
            {
                if (!(parentExecution.ParentId is null))
                {
                    parentExecution = parentExecution.Parent;
                }
                else
                {
                    parentExecution = parentExecution.SuperExecution;
                }
            }

            // Collect all child executions now we have the parent
            IList<IExecutionEntity> allExecutions = new List<IExecutionEntity>
            {
                parentExecution
            };
            CollectChildExecutions(parentExecution, allExecutions);
            return BuildExecutionTree(allExecutions);
        }

        protected internal static void CollectChildExecutions(IExecutionEntity rootExecutionEntity, IList<IExecutionEntity> allExecutions)
        {
            foreach (IExecutionEntity childExecutionEntity in rootExecutionEntity.Executions)
            {
                allExecutions.Add(childExecutionEntity);
                CollectChildExecutions(childExecutionEntity, allExecutions);
            }

            if (rootExecutionEntity.SubProcessInstance != null)
            {
                allExecutions.Add(rootExecutionEntity.SubProcessInstance);
                CollectChildExecutions(rootExecutionEntity.SubProcessInstance, allExecutions);
            }
        }

        public static ExecutionTree BuildExecutionTree(ICollection<IExecutionEntity> executions)
        {
            ExecutionTree executionTree = new ExecutionTree();

            // Map the executions to their parents. Catch and store the root element (process instance execution) while were at it
            IDictionary<string, IList<IExecutionEntity>> parentMapping = new Dictionary<string, IList<IExecutionEntity>>();
            foreach (IExecutionEntity executionEntity in executions)
            {
                string parentId = executionEntity.ParentId;

                // Support for call activity
                if (parentId is null)
                {
                    parentId = ((IExecution)executionEntity).SuperExecutionId;
                }

                if (!(parentId is null))
                {
                    if (!parentMapping.ContainsKey(parentId))
                    {
                        parentMapping[parentId] = new List<IExecutionEntity>();
                    }
                    parentMapping[parentId].Add(executionEntity);
                }
                else if (((IExecution)executionEntity).SuperExecutionId is null)
                {
                    executionTree.Root = new ExecutionTreeNode(executionEntity);
                }
            }

            FillExecutionTree(executionTree, parentMapping);
            return executionTree;
        }

        public static ExecutionTree BuildExecutionTreeForProcessInstance(ICollection<IExecutionEntity> executions)
        {
            ExecutionTree executionTree = new ExecutionTree();
            if (executions.Count == 0)
            {
                return executionTree;
            }

            // Map the executions to their parents. Catch and store the root element (process instance execution) while were at it
            IDictionary<string, IList<IExecutionEntity>> parentMapping = new Dictionary<string, IList<IExecutionEntity>>();
            foreach (IExecutionEntity executionEntity in executions)
            {
                string parentId = executionEntity.ParentId;

                if (!(parentId is null))
                {
                    if (!parentMapping.ContainsKey(parentId))
                    {
                        parentMapping[parentId] = new List<IExecutionEntity>();
                    }
                    parentMapping[parentId].Add(executionEntity);
                }
                else
                {
                    executionTree.Root = new ExecutionTreeNode(executionEntity);
                }
            }

            FillExecutionTree(executionTree, parentMapping);
            return executionTree;
        }

        private static void FillExecutionTree(ExecutionTreeNode parentNode, IDictionary<string, IList<IExecutionEntity>> parentMapping)
        {
            string parentId = parentNode.ExecutionEntity.Id;
            if (parentMapping.ContainsKey(parentId))
            {
                IList<IExecutionEntity> childExecutions = parentMapping[parentId];
                IList<ExecutionTreeNode> childNodes = new List<ExecutionTreeNode>(childExecutions.Count);

                foreach (IExecutionEntity childExecutionEntity in childExecutions)
                {
                    ExecutionTreeNode childNode = new ExecutionTreeNode(childExecutionEntity)
                    {
                        Parent = parentNode
                    };
                    childNodes.Add(childNode);

                    FillExecutionTree(childNode, parentMapping);
                }

                parentNode.Children = childNodes;
            }
        }

        protected internal static void FillExecutionTree(ExecutionTree executionTree, IDictionary<string, IList<IExecutionEntity>> parentMapping)
        {
            if (executionTree.Root == null)
            {
                throw new ActivitiException("Programmatic error: the list of passed executions in the argument of the method should contain the process instance execution");
            }

            FillExecutionTree(executionTree.Root, parentMapping);
        }
    }
}