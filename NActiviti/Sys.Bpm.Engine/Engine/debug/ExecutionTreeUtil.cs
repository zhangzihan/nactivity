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
namespace org.activiti.engine.debug
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.runtime;

    /// 
    public class ExecutionTreeUtil
    {

        public static ExecutionTree buildExecutionTree(IExecutionEntity executionEntity)
        {

            // Find highest parent
            IExecutionEntity parentExecution = executionEntity;
            while (!ReferenceEquals(parentExecution.ParentId, null) || !ReferenceEquals(((IExecution)parentExecution).SuperExecutionId, null))
            {
                if (!ReferenceEquals(parentExecution.ParentId, null))
                {
                    parentExecution = parentExecution.Parent;
                }
                else
                {
                    parentExecution = parentExecution.SuperExecution;
                }
            }

            // Collect all child executions now we have the parent
            IList<IExecutionEntity> allExecutions = new List<IExecutionEntity>();
            allExecutions.Add(parentExecution);
            collectChildExecutions(parentExecution, allExecutions);
            return buildExecutionTree(allExecutions);
        }

        protected internal static void collectChildExecutions(IExecutionEntity rootExecutionEntity, IList<IExecutionEntity> allExecutions)
        {
            foreach (IExecutionEntity childExecutionEntity in rootExecutionEntity.Executions)
            {
                allExecutions.Add(childExecutionEntity);
                collectChildExecutions(childExecutionEntity, allExecutions);
            }

            if (rootExecutionEntity.SubProcessInstance != null)
            {
                allExecutions.Add(rootExecutionEntity.SubProcessInstance);
                collectChildExecutions(rootExecutionEntity.SubProcessInstance, allExecutions);
            }
        }

        public static ExecutionTree buildExecutionTree(ICollection<IExecutionEntity> executions)
        {
            ExecutionTree executionTree = new ExecutionTree();

            // Map the executions to their parents. Catch and store the root element (process instance execution) while were at it
            IDictionary<string, IList<IExecutionEntity>> parentMapping = new Dictionary<string, IList<IExecutionEntity>>();
            foreach (IExecutionEntity executionEntity in executions)
            {
                string parentId = executionEntity.ParentId;

                // Support for call activity
                if (ReferenceEquals(parentId, null))
                {
                    parentId = ((IExecution)executionEntity).SuperExecutionId;
                }

                if (!ReferenceEquals(parentId, null))
                {
                    if (!parentMapping.ContainsKey(parentId))
                    {
                        parentMapping[parentId] = new List<IExecutionEntity>();
                    }
                    parentMapping[parentId].Add(executionEntity);
                }
                else if (ReferenceEquals(((IExecution)executionEntity).SuperExecutionId, null))
                {
                    executionTree.Root = new ExecutionTreeNode(executionEntity);
                }
            }

            fillExecutionTree(executionTree, parentMapping);
            return executionTree;
        }

        public static ExecutionTree buildExecutionTreeForProcessInstance(ICollection<IExecutionEntity> executions)
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

                if (!ReferenceEquals(parentId, null))
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

            fillExecutionTree(executionTree, parentMapping);
            return executionTree;
        }

        protected internal static void fillExecutionTree(ExecutionTree executionTree, IDictionary<string, IList<IExecutionEntity>> parentMapping)
        {
            if (executionTree.Root == null)
            {
                throw new ActivitiException("Programmatic error: the list of passed executions in the argument of the method should contain the process instance execution");
            }

            // Now build the tree, top-down
            LinkedList<ExecutionTreeNode> executionsToHandle = new LinkedList<ExecutionTreeNode>();
            executionsToHandle.AddLast(executionTree.Root);
            var @iterator = executionsToHandle.GetEnumerator();
            while (@iterator.MoveNext())
            {
                ExecutionTreeNode parentNode = @iterator.Current;
                string parentId = parentNode.ExecutionEntity.Id;
                if (parentMapping.ContainsKey(parentId))
                {
                    IList<IExecutionEntity> childExecutions = parentMapping[parentId];
                    IList<ExecutionTreeNode> childNodes = new List<ExecutionTreeNode>(childExecutions.Count);

                    foreach (IExecutionEntity childExecutionEntity in childExecutions)
                    {

                        ExecutionTreeNode childNode = new ExecutionTreeNode(childExecutionEntity);
                        childNode.Parent = parentNode;
                        childNodes.Add(childNode);

                        executionsToHandle.AddLast(childNode);
                    }

                    parentNode.Children = childNodes;

                }
            }
        }

    }
}