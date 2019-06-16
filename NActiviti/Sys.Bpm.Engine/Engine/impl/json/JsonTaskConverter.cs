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
namespace Sys.Workflow.Engine.Impl.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Sys.Workflow.Engine.Impl.Persistence.Entity;
    using Sys.Workflow.Engine.Tasks;
    using System.IO;

    /// 
    public class JsonTaskConverter : JsonObjectConverter<ITask>
    {

        public override ITask ToObject(StreamReader reader)
        {
            string str = reader.ReadToEnd();
            ITask task = JsonConvert.DeserializeObject<TaskEntityImpl>(str);

            return task;
            //throw new ActivitiException("not yet implemented");
        }

        public override JToken ToJsonObject(ITask task)
        {
            ITaskEntity taskEntity = (ITaskEntity)task;
            JToken jsonObject = JToken.FromObject(new
            {
                id = taskEntity.Id,
                dbversion = taskEntity.Revision,
                assignee = taskEntity.Assignee,
                name = taskEntity.Name,
                priority = taskEntity.Priority,
                createTime = taskEntity.CreateTime
            });

            if (!string.IsNullOrWhiteSpace(taskEntity.ExecutionId))
            {
                jsonObject["activityInstance"] = taskEntity.ExecutionId;
            }
            if (!string.IsNullOrWhiteSpace(taskEntity.ProcessDefinitionId))
            {
                jsonObject["processDefinition"] = taskEntity.ProcessDefinitionId;
            }
            return jsonObject;
        }
    }

}