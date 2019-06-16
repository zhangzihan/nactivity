using Newtonsoft.Json;
using System;

/*
 * Copyright 2018 Alfresco, Inc. and/or its affiliates.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Sys.Workflow.Cloud.Services.Api.Commands
{
    /// <summary>
    /// 追加审批人员
    /// </summary>
    public class AppendCountersignCmd : ICommand
    {
        private readonly string id = "appendCountersignCmd";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="description">任务描述</param>
        /// <param name="dueDate">过期日期</param>
        /// <param name="priority">任务优先级</param>
        /// <param name="assignees">追加人员列表</param>
        /// <param name="taskId">任务id</param>
        ////[JsonConstructor]
        public AppendCountersignCmd([JsonProperty("Name")] string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("DueDate")] DateTime? dueDate,
            [JsonProperty("Priority")]int? priority,
            [JsonProperty("Assignees")]string[] assignees,
            [JsonProperty("TaskId")]string taskId)
        {
            this.id = Guid.NewGuid().ToString();
            this.Name = name;
            this.Description = description;
            this.DueDate = dueDate;
            this.Priority = priority;
            this.Assignees = assignees;
            this.TaskId = taskId;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get => id;
        }

        /// <summary>
        /// 任务名称
        /// </summary>

        public virtual string Name
        {
            get;
            set;
        }

        /// <summary>
        /// 任务描述
        /// </summary>

        public virtual string Description
        {
            get;
            set;
        }

        /// <summary>
        /// 过期日期
        /// </summary>

        public virtual DateTime? DueDate
        {
            get;
            set;
        }

        /// <summary>
        /// 任务优先级
        /// </summary>

        public virtual int? Priority
        {
            get;
            set;
        }

        /// <summary>
        /// 追加人员列表
        /// </summary>

        public virtual string[] Assignees
        {
            get;
            set;
        }

        /// <summary>
        /// 任务id
        /// </summary>

        public virtual string TaskId
        {
            get;
            set;
        }
    }

}