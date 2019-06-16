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
namespace Sys.Workflow.Engine.Impl.Delegate.Invocation
{
    using Sys.Workflow.Engine.Delegate;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// <summary>
    /// Class handling invocations of JavaDelegates
    /// 
    /// 
    /// </summary>
    public class CSharpDelegateInvocation : DelegateInvocation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal readonly ICSharpDelegate delegateInstance;

        /// <summary>
        /// 
        /// </summary>
        protected internal readonly IExecutionEntity execution;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegateInstance"></param>
        /// <param name="execution"></param>
        public CSharpDelegateInvocation(ICSharpDelegate delegateInstance, IExecutionEntity execution)
        {
            this.delegateInstance = delegateInstance;
            this.execution = execution;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void Invoke()
        {
            delegateInstance.Execute(execution);
        }

        /// <summary>
        /// 
        /// </summary>
        public override object Target
        {
            get
            {
                return delegateInstance;
            }
        }
    }
}