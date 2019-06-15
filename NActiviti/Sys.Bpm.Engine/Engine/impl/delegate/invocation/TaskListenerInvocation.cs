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
namespace org.activiti.engine.impl.@delegate.invocation
{
    using org.activiti.engine.@delegate;

    /// <summary>
    /// Class handling invocations of <seealso cref="ITaskListener TaskListeners"/>
    /// 
    /// 
    /// </summary>
    public class TaskListenerInvocation : DelegateInvocation
    {
        /// <summary>
        /// 
        /// </summary>
        protected internal readonly ITaskListener executionListenerInstance;
        /// <summary>
        /// 
        /// </summary>
        protected internal readonly IDelegateTask delegateTask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionListenerInstance"></param>
        /// <param name="delegateTask"></param>
        public TaskListenerInvocation(ITaskListener executionListenerInstance, IDelegateTask delegateTask)
        {
            this.executionListenerInstance = executionListenerInstance;
            this.delegateTask = delegateTask;
        }

        /// <summary>
        /// 
        /// </summary>
        protected internal override void Invoke()
        {
            executionListenerInstance.Notify(delegateTask);
        }

        /// <summary>
        /// 
        /// </summary>
        public override object Target
        {
            get
            {
                return executionListenerInstance;
            }
        }
    }
}