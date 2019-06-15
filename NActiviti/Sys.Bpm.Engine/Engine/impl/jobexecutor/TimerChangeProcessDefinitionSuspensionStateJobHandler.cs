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
namespace org.activiti.engine.impl.jobexecutor
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.interceptor;

    /// 
    public abstract class TimerChangeProcessDefinitionSuspensionStateJobHandler : IJobHandler
    {
        public abstract void Execute(org.activiti.engine.impl.persistence.entity.IJobEntity job, string configuration, org.activiti.engine.impl.persistence.entity.IExecutionEntity execution, ICommandContext commandContext);
        public abstract string Type { get; }

        private const string JOB_HANDLER_CFG_INCLUDE_PROCESS_INSTANCES = "includeProcessInstances";

        public static string CreateJobHandlerConfiguration(bool includeProcessInstances)
        {
            JToken json = new JObject
            {
                [JOB_HANDLER_CFG_INCLUDE_PROCESS_INSTANCES] = includeProcessInstances
            };

            return json.ToString();
        }

        public static bool GetIncludeProcessInstances(JToken jobHandlerCfgJson)
        {
            if (bool.TryParse(jobHandlerCfgJson[JOB_HANDLER_CFG_INCLUDE_PROCESS_INSTANCES]?.ToString(), out var b))
            {
                return b;
            }

            return false;
        }

    }

}