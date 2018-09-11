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
namespace org.activiti.engine.impl.json
{
    using Newtonsoft.Json.Linq;
    using org.activiti.engine.impl.persistence.entity;
    using org.activiti.engine.task;
    using System.IO;

    /// 
    public class JsonTaskConverter : JsonObjectConverter<ITask>
    {

        public override ITask toObject(StreamReader reader)
        {
            throw new ActivitiException("not yet implemented");
        }

        public override JToken toJsonObject(ITask task)
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