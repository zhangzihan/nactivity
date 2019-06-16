using System;

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

namespace Sys.Workflow.engine.impl.bpmn.behavior
{
    using Sys.Workflow.engine.@delegate;
    using Sys.Workflow.engine.impl.context;
    using Sys.Workflow.engine.impl.@delegate;
    using Sys.Workflow.engine.impl.@delegate.invocation;
    using Sys.Workflow.engine.impl.persistence.entity;

    /// 
    [Serializable]
    public class ServiceTaskJavaDelegateActivityBehavior : TaskActivityBehavior, IActivityBehavior, IExecutionListener
    {

        private const long serialVersionUID = 1L;

        protected internal IJavaDelegate javaDelegate;

        protected internal ServiceTaskJavaDelegateActivityBehavior()
        {
        }

        public ServiceTaskJavaDelegateActivityBehavior(IJavaDelegate javaDelegate)
        {
            this.javaDelegate = javaDelegate;
        }

        public override void Execute(IExecutionEntity execution)
        {
            Context.ProcessEngineConfiguration.DelegateInterceptor.HandleInvocation(new JavaDelegateInvocation(javaDelegate, execution));
            Leave(execution);
        }

        public virtual void Notify(IExecutionEntity execution)
        {
            Execute(execution);
        }
    }

}