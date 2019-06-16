using Newtonsoft.Json;
using Sys.Workflow.services.api.commands;
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

namespace Sys.Workflow.cloud.services.api.commands
{


    /// <summary>
    /// 任务更新命令
    /// </summary>
    public class UpdateTaskCmd : ICommand, IUpdateTaskCmd
    {

        private readonly string id = "updateTaskCmd";
        private string name;
        private string description;
        private DateTime? dueDate;
        private int? priority;
        private string assignee;
        private string parentTaskId;
        private string tenantId;
        private string taskId;


        /// <summary>
        /// 构造函数
        /// </summary>
        public UpdateTaskCmd()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="name">任务名称</param>
        /// <param name="description">任务描述</param>
        /// <param name="dueDate">过期日期</param>
        /// <param name="priority">任务优先级</param>
        /// <param name="assignee">任务分配人id</param>
        /// <param name="parentTaskId">父级任务id</param>
        /// <param name="tenantId">租户id</param>
        //[JsonConstructor]
        public UpdateTaskCmd([JsonProperty("TaskId")] string taskId,
            [JsonProperty("Name")] string name,
            [JsonProperty("Description")] string description,
            [JsonProperty("DueDate")] DateTime? dueDate,
            [JsonProperty("Priority")] int? priority,
            [JsonProperty("Assignee")] string assignee,
            [JsonProperty("ParentTaskId")] string parentTaskId,
            [JsonProperty("TenantId")] string tenantId) : this()
        {
            this.taskId = taskId;
            this.name = name;
            this.description = description;
            this.dueDate = dueDate;
            this.priority = priority;
            this.assignee = assignee;
            this.parentTaskId = parentTaskId;
            this.tenantId = tenantId;
        }

        /// <summary>
        /// 命令id
        /// </summary>

        public virtual string Id
        {
            get => id;
        }

        /// <summary>
        /// 任务id
        /// </summary>
        public virtual string TaskId
        {
            get => taskId;
            set => taskId = value;
        }

        /// <summary>
        /// 任务名称
        /// </summary>

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => name = value;
        }

        /// <summary>
        /// 任务描述
        /// </summary>

        public virtual string Description
        {
            get
            {
                return description;
            }
            set => description = value;
        }

        /// <summary>
        /// 过期日期
        /// </summary>

        public virtual DateTime? DueDate
        {
            get
            {
                return dueDate;
            }
            set => dueDate = value;
        }

        /// <summary>
        /// 任务优先级
        /// </summary>

        public virtual int? Priority
        {
            get
            {
                return priority;
            }
            set => priority = value;
        }

        /// <summary>
        /// 任务分配人id
        /// </summary>

        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set => assignee = value;
        }

        /// <summary>
        /// 父级任务id
        /// </summary>

        public virtual string ParentTaskId
        {
            get
            {
                return parentTaskId;
            }
            set => parentTaskId = value;
        }

        /// <summary>
        /// 租户id
        /// </summary>
        public virtual string TenantId
        {
            get => tenantId;
            set => tenantId = value;
        }
    }

}