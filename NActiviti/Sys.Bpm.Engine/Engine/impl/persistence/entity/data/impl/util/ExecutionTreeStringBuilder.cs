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
namespace Sys.Workflow.Engine.Impl.Persistence.Entity.Data.Impl.Util
{

    /// <summary>
    /// Prints a nicely tree-looking overview of the executions.
    /// 
    /// 
    /// </summary>
    public class ExecutionTreeStringBuilder
    {

        protected internal IExecutionEntity executionEntity;

        public ExecutionTreeStringBuilder(IExecutionEntity executionEntity)
        {
            this.executionEntity = executionEntity;
        }

        /* See http://stackoverflow.com/questions/4965335/how-to-print-binary-tree-diagram */
        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            strb.Append(executionEntity.Id).Append(" : ").Append(executionEntity.ActivityId).Append(", parent id ").Append(executionEntity.ParentId).AppendLine();

            IList<IExecutionEntity> children = executionEntity.Executions;
            if (children != null)
            {
                foreach (IExecutionEntity childExecution in children)
                {
                    InternalToString(childExecution, strb, "", true);
                }
            }
            return strb.ToString();
        }

        protected internal virtual void InternalToString(IExecutionEntity execution, StringBuilder strb, string prefix, bool isTail)
        {
            strb.Append(prefix).Append(isTail ? "└── " : "├── ").Append(execution.Id).Append(" : ").Append("activityId=" + execution.ActivityId).Append(", parent id ").Append(execution.ParentId).Append(execution.IsScope ? " (scope)" : "").Append(execution.IsMultiInstanceRoot ? " (multi instance root)" : "").AppendLine();

            IList<IExecutionEntity> children = executionEntity.Executions;
            if (children != null)
            {
                for (int i = 0; i < children.Count - 1; i++)
                {
                    InternalToString(children[i], strb, prefix + (isTail ? "    " : "│   "), false);
                }
                if (children.Count > 0)
                {
                    InternalToString(children[children.Count - 1], strb, prefix + (isTail ? "    " : "│   "), true);
                }
            }
        }

    }
}