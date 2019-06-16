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

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    using Microsoft.Extensions.Logging;
    using Sys.Workflow.Engine.Impl.Bpmn.Helper;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow;

    /// <summary>
    /// ActivityBehavior that evaluates an expression when executed. Optionally, it sets the result of the expression as a variable on the execution.
    /// 
    /// 
    /// 
    /// 
    /// 
    /// 
    /// </summary>
    [Serializable]
    public class ServiceTaskLoggerActivityBehavior : TaskActivityBehavior
    {
        private const long serialVersionUID = 1L;

        private static readonly ILogger logger = ProcessEngineServiceProvider.LoggerService<ServiceTaskLoggerActivityBehavior>();

        public override void Execute(IExecutionEntity execution)
        {
            try
            {
                execution.WriteDebugLog();

                Leave(execution);
            }
            catch (Exception exc)
            {
                logger.LogError($"execution {execution.ActivityId} failed,exception: {exc.Message}");
            }
        }
    }
}