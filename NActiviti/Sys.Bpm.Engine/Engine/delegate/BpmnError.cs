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

namespace Sys.Workflow.engine.@delegate
{
    using Sys.Workflow.engine.impl.bpmn.parser;

    /// <summary>
    /// Special exception that can be used to throw a BPMN Error from <seealso cref="IJavaDelegate"/>s and expressions.
    /// 
    /// This should only be used for business faults, which shall be handled by a Boundary Error Event or Error Event Sub-Process modeled in the process definition. Technical errors should be represented
    /// by other exception types.
    /// 
    /// This class represents an actual instance of a BPMN Error, whereas <seealso cref="Error"/> represents an Error definition.
    /// 
    /// 
    /// </summary>
    public class BpmnError : ActivitiException
    {

        private const long serialVersionUID = 1L;

        private string errorCode;

        public BpmnError(string errorCode) : base("")
        {
            ErrorCode = errorCode;
        }

        public BpmnError(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        protected internal virtual string ErrorCode
        {
            set
            {
                if (ReferenceEquals(value, null))
                {
                    throw new ActivitiIllegalArgumentException("Error Code must not be null.");
                }
                if (value.Length < 1)
                {
                    throw new ActivitiIllegalArgumentException("Error Code must not be empty.");
                }
                this.errorCode = value;
            }
            get
            {
                return errorCode;
            }
        }

    }

}