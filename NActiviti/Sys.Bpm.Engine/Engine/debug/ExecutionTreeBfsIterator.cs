using System.Collections;
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

    /// <summary>
    /// Iterates over an <seealso cref="ExecutionTree"/> using breadth-first search
    /// 
    /// 
    /// </summary>
    public class ExecutionTreeBfsIterator : IEnumerator<ExecutionTreeNode>
    {
        protected internal ExecutionTreeNode rootNode;
        protected internal bool reverseOrder;

        protected internal LinkedList<ExecutionTreeNode> flattenedList;
        protected internal IEnumerator<ExecutionTreeNode> flattenedListIterator;

        public ExecutionTreeNode Current => flattenedListIterator.Current;

        object IEnumerator.Current => flattenedListIterator.Current;

        public ExecutionTreeBfsIterator(ExecutionTreeNode executionTree) : this(executionTree, false)
        {
        }

        public ExecutionTreeBfsIterator(ExecutionTreeNode rootNode, bool reverseOrder)
        {
            this.rootNode = rootNode;
            this.reverseOrder = reverseOrder;
        }

        protected internal virtual void FlattenTree()
        {
            flattenedList = new LinkedList<ExecutionTreeNode>();

            LinkedList<ExecutionTreeNode> nodesToHandle = new LinkedList<ExecutionTreeNode>();
            nodesToHandle.AddLast(rootNode);
            var @iterator = nodesToHandle.GetEnumerator();
            while (iterator.MoveNext())
            {

                ExecutionTreeNode currentNode = iterator.Current;
                if (reverseOrder)
                {
                    flattenedList.AddFirst(currentNode);
                }
                else
                {
                    flattenedList.AddLast(currentNode);
                }

                if (currentNode.Children != null && currentNode.Children.Count > 0)
                {
                    foreach (ExecutionTreeNode childNode in currentNode.Children)
                    {
                        nodesToHandle.AddLast(childNode);
                    }
                }
            }

            flattenedListIterator = flattenedList.GetEnumerator();
        }

        public bool HasNext()
        {
            if (flattenedList == null)
            {
                FlattenTree();
            }

            return true; //flattenedListIterator.hasNext();
        }

        public ExecutionTreeNode Next()
        {
            if (MoveNext())
            {
                return flattenedListIterator.Current;
            }

            return null;
        }

        public void Remove()
        {
            if (flattenedList == null)
            {
                FlattenTree();
            }

            if (flattenedListIterator.Current != null)
            {
                flattenedList.Remove(flattenedListIterator.Current);
            }
        }

        public bool MoveNext()
        {
            if (flattenedList == null)
            {
                FlattenTree();
            }

            return flattenedListIterator.MoveNext();
        }

        public void Reset()
        {
            flattenedListIterator.Reset();
        }

        public void Dispose()
        {
            flattenedListIterator.Dispose();
        }
    }
}