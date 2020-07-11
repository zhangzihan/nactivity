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
namespace Sys.Workflow.Engine.Impl.Interceptor
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Agenda;
    using Sys.Workflow;
    using Sys.Workflow.Engine.Impl.Contexts;
    using Sys.Workflow.Engine.Impl.Cfg;
    using Sys.Workflow.Engine.Impl.Bpmn.Listener;
    using System;
    using Sys.Workflow.Exceptions;
    using Sys.Workflow.Engine.Impl.Identities;
    using Sys.Workflow.Engine.Debug;

    /// 
    public class DebugCommandInvoker : CommandInvoker
    {
        public override void ExecuteOperation(AbstractOperation runnable)
        {
            try
            {
                ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;
                if (processEngineConfiguration.EnableEventDispatcher)
                {
                    if (runnable.Execution != null)
                    {
                        processEngineConfiguration.EventDispatcher.DispatchEvent(new WorkflowDebuggerEvent(runnable.Execution));
                    }
                }

                base.ExecuteOperation(runnable);
            }
            catch (Exception ex)
            {
                throw new WorkflowDebugException(Authentication.AuthenticatedUser?.Id, runnable.Execution, ex);
            }
        }
    }
}