using Newtonsoft.Json.Linq;
using org.activiti.engine.impl.persistence.entity;
using org.activiti.engine.task;
using Sys.Net.Http;
using System;
using System.Collections.Generic;
using static org.activiti.cloud.services.api.model.TaskModel;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
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
 *
 */

namespace org.activiti.cloud.services.api.model.converter
{

    /// <summary>
    /// 
    /// </summary>
    public class TaskConverter : IModelConverter<ITask, TaskModel>
    {

        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual TaskModel From(ITask source)
        {
            TaskModel task = null;
            if (source != null)
            {
                task = new TaskModel(source.Id,
                    source.Owner,
                    source.Assignee,
                    source.Name,
                    source.Description,
                    source.CreateTime,
                    null,
                    source.ClaimTime,
                    source.DueDate,
                    source.Priority,
                    source.ProcessDefinitionId,
                    source.ProcessInstanceId,
                    source.ParentTaskId,
                    source.FormKey,
                    CalculateStatus(source),
                    null,
                    source.IsTransfer,
                    source.CanTransfer,
                    source.OnlyAssignee,
                    source.BusinessKey);

                if (source.Assigner != null)
                {
                    task.Assigner = JToken.FromObject(source.Assigner).ToObject<UserInfo>();
                }
            }
            return task;
        }


        /// <summary>
        /// 
        /// </summary>
        private string CalculateStatus(ITask source)
        {
            if (source is ITaskEntity && (((ITaskEntity)source).Deleted || ((ITaskEntity)source).Canceled))
            {
                return Enum.GetName(typeof(TaskStatus), TaskStatus.CANCELLED);
            }
            else if (((ITaskEntity)source).Suspended)
            {
                return Enum.GetName(typeof(TaskStatus), TaskStatus.SUSPENDED);
            }
            else if (!string.IsNullOrWhiteSpace(source.Assignee))
            {
                return Enum.GetName(typeof(TaskStatus), TaskStatus.ASSIGNED);
            }
            return Enum.GetName(typeof(TaskStatus), TaskStatus.CREATED);
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IEnumerable<TaskModel> From(IEnumerable<ITask> tasks)
        {
            return listConverter.From(tasks, this);
        }
    }

}