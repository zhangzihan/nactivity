using Newtonsoft.Json;
using Sys.Workflow.Services.Api.Commands;
using System;
using System.Collections.Generic;

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
    /// 任务转办命令
    /// </summary>
    public class TransferTaskCmd : ICommand, ITransferTaskCmd
    {
        private readonly string id = "transferTaskCmd";
        private string _description;

        /// <summary>
        /// TransferTaskCmd
        /// </summary>
        public TransferTaskCmd()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="description">任务描述</param>
        /// <param name="dueDate">过期日期</param>
        /// <param name="priority">任务优先级</param>
        /// <param name="assignees">转办人员列表</param>
        /// <param name="taskId">转办的任务id</param>
        /// <param name="tenantId">租户id</param>
        /// <param name="variables">流程变量</param>
        ////[JsonConstructor]
        public TransferTaskCmd([JsonProperty("Name")] string name,
            [JsonProperty("Description")] string description,
            [JsonProperty("DueDate")] DateTime? dueDate,
            [JsonProperty("Priority")] int? priority,
            [JsonProperty("Assignees")] string[] assignees,
            [JsonProperty("TaskId")] string taskId,
            [JsonProperty("TenantId")] string tenantId,
            [JsonProperty("Variables")] WorkflowVariable variables)
        {
            this.Name = name;
            this.Description = description;
            this.DueDate = dueDate;
            this.Priority = priority;
            this.Assignees = assignees;
            this.TaskId = taskId;
            this.TenantId = tenantId;
            this.Variables = variables;
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
            get
            {
                return
#if DEBUG
                    _description = string.IsNullOrWhiteSpace(_description) ? "任务已转派" : _description;
#else
                _description;
#endif
            }
            set => _description = value;
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
        /// 转办人员列表
        /// </summary>

        public virtual string[] Assignees
        {
            get;
            set;
        }

        /// <summary>
        /// 上级任务id
        /// </summary>

        public virtual string TaskId
        {
            get;
            set;
        }

        /// <inheritdoc />
        public virtual string TenantId
        {
            get;
            set;
        }

        /// <inheritdoc />
        public WorkflowVariable Variables
        {
            get; set;
        }

        public IDictionary<string, object> TransientVariables
        {
            get; set;
        }
    }
}