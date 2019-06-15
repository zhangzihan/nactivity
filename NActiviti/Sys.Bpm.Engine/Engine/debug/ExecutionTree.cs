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
    using System.Collections;
    using org.activiti.engine.impl.persistence.entity;

    /// 
    public class ExecutionTree : IEnumerable<ExecutionTreeNode>
    {

        protected internal ExecutionTreeNode root;

        public ExecutionTree()
        {

        }

        public virtual ExecutionTreeNode Root
        {
            get
            {
                return root;
            }
            set
            {
                this.root = value;
            }
        }


        /// <summary>
        /// Looks up the <seealso cref="IExecutionEntity"/> for a given id.
        /// </summary>
        public virtual ExecutionTreeNode GetTreeNode(string executionId)
        {
            return GetTreeNode(executionId, root);
        }

        protected internal virtual ExecutionTreeNode GetTreeNode(string executionId, ExecutionTreeNode currentNode)
        {
            if (currentNode.ExecutionEntity.Id.Equals(executionId))
            {
                return currentNode;
            }

            IList<ExecutionTreeNode> children = currentNode.Children;
            if (currentNode.Children != null && children.Count > 0)
            {
                int index = 0;
                while (index < children.Count)
                {
                    ExecutionTreeNode result = GetTreeNode(executionId, children[index]);
                    if (result != null)
                    {
                        return result;
                    }
                    index++;
                }
            }

            return null;
        }

        public virtual IEnumerator<ExecutionTreeNode> GetEnumerator()
        {
            return new ExecutionTreeBfsIterator(this.Root);
        }

        public virtual ExecutionTreeBfsIterator BfsIterator()
        {
            return new ExecutionTreeBfsIterator(this.Root);
        }

        /// <summary>
        /// Uses an <seealso cref="ExecutionTreeBfsIterator"/>, but returns the leafs first (so flipped order of BFS)
        /// </summary>
        public virtual ExecutionTreeBfsIterator LeafsFirstIterator()
        {
            return new ExecutionTreeBfsIterator(this.Root, true);
        }

        public override string ToString()
        {
            return root != null ? root.ToString() : "";
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}