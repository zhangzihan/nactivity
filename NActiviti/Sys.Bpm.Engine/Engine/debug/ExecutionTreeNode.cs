using System;
using System.Collections.Generic;
using System.Text;

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
namespace Sys.Workflow.Engine.Debug
{
    using System.Collections;
    using Sys.Workflow.Bpmn.Models;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class ExecutionTreeNode : IEnumerable<ExecutionTreeNode>
    {

        protected internal IExecutionEntity executionEntity;
        protected internal ExecutionTreeNode parent;
        protected internal IList<ExecutionTreeNode> children;

        public ExecutionTreeNode(IExecutionEntity executionEntity)
        {
            this.executionEntity = executionEntity;
        }

        public virtual IExecutionEntity ExecutionEntity
        {
            get
            {
                return executionEntity;
            }
            set
            {
                this.executionEntity = value;
            }
        }


        public virtual ExecutionTreeNode Parent
        {
            get
            {
                return parent;
            }
            set
            {
                this.parent = value;
            }
        }


        public virtual IList<ExecutionTreeNode> Children
        {
            get
            {
                return children;
            }
            set
            {
                this.children = value;
            }
        }


        IEnumerator<ExecutionTreeNode> IEnumerable<ExecutionTreeNode>.GetEnumerator()
        {
            return children?.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children?.GetEnumerator();
        }

        public virtual ExecutionTreeBfsIterator LeafsFirstIterator()
        {
            return new ExecutionTreeBfsIterator(this, true);
        }

        /* See http://stackoverflow.com/questions/4965335/how-to-print-binary-tree-diagram */
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            strb.Append(ExecutionEntity.Id);
            if (ExecutionEntity.ActivityId is object)
            {
                strb.Append(" : " + ExecutionEntity.ActivityId);
            }
            if (ExecutionEntity.ParentId is object)
            {
                strb.Append(", parent id " + ExecutionEntity.ParentId);
            }
            if (ExecutionEntity.ProcessInstanceType)
            {
                strb.Append(" (process instance)");
            }
            strb.Append(System.Environment.NewLine);
            if (children != null)
            {
                foreach (ExecutionTreeNode childNode in children)
                {
                    childNode.InternalToString(strb, "", true);
                }
            }
            return strb.ToString();
        }

        protected internal virtual void InternalToString(StringBuilder strb, string prefix, bool isTail)
        {
            strb.Append(prefix + (isTail ? "└── " : "├── ") + ExecutionEntity.Id + " : " + CurrentFlowElementId + ", parent id " + ExecutionEntity.ParentId + (ExecutionEntity.IsActive ? " (active)" : " (not active)") + (ExecutionEntity.IsScope ? " (scope)" : "") + (ExecutionEntity.IsMultiInstanceRoot ? " (multi instance root)" : "") + (ExecutionEntity.Ended ? " (ended)" : "") + System.Environment.NewLine);
            if (children != null)
            {
                for (int i = 0; i < children.Count - 1; i++)
                {
                    children[i].InternalToString(strb, prefix + (isTail ? "    " : "│   "), false);
                }
                if (children.Count > 0)
                {
                    children[children.Count - 1].InternalToString(strb, prefix + (isTail ? "    " : "│   "), true);
                }
            }
        }

        protected internal virtual string CurrentFlowElementId
        {
            get
            {
                FlowElement flowElement = ExecutionEntity.CurrentFlowElement;
                if (flowElement is SequenceFlow sequenceFlow)
                {
                    return sequenceFlow.SourceRef + " -> " + sequenceFlow.TargetRef;
                }
                else if (flowElement != null)
                {
                    return flowElement.Id + " (" + flowElement.GetType().Name;
                }
                else
                {
                    return "";
                }
            }
        }
    }
}