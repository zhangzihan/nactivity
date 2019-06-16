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
namespace Sys.Workflow.Engine.Impl.JobExecutors
{
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Cmd;
    using Sys.Workflow.Engine.Impl.Interceptor;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;

    /// 
    public class TimerSuspendProcessDefinitionHandler : TimerChangeProcessDefinitionSuspensionStateJobHandler
    {

        public const string TYPE = "suspend-processdefinition";

        public override string Type
        {
            get
            {
                return TYPE;
            }
        }

        public JToken JTokenconfiguration { get; private set; }

        public override void Execute(IJobEntity job, string configuration, IExecutionEntity execution, ICommandContext commandContext)
        {
            JToken cfgJson = JToken.FromObject(configuration);
            string processDefinitionId = job.ProcessDefinitionId;
            bool suspendProcessInstances = GetIncludeProcessInstances(cfgJson);

            SuspendProcessDefinitionCmd suspendProcessDefinitionCmd = new SuspendProcessDefinitionCmd(processDefinitionId, null, suspendProcessInstances, null, job.TenantId);
            suspendProcessDefinitionCmd.Execute(commandContext);
        }

    }

}