using Newtonsoft.Json;
using Sys.Workflow.Services.Api.Commands;
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
    /// 任务更新命令
    /// </summary>
    public class ReturnToTaskCmd : ICommand
    {
        private readonly string id = "returnToTaskCmd";

        public ReturnToTaskCmd()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="returnReason">回退原因</param>
        /// <param name="variables">任务附件变量</param>
        /// <param name="tenantId">租户ID</param>
        //[JsonConstructor]
        public ReturnToTaskCmd([JsonProperty("TaskId")] string taskId,
            [JsonProperty("ActivityId")] string activityId,
            [JsonProperty("ReturnReason")] string returnReason,
            [JsonProperty("TenantId")] string tenantId,
            [JsonProperty("Variables")] WorkflowVariable variables)
        {
            this.TaskId = taskId;
            this.ReturnReason = returnReason;
            this.ActivityId = activityId;
            this.Variables = variables;
            this.TenantId = tenantId;
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
        public virtual string TaskId { get; set; }

        /// <summary>
        /// 退回任务节点id
        /// </summary>
        public virtual string ActivityId { get; set; }

        /// <summary>
        /// 退回原因
        /// </summary>

        public virtual string ReturnReason { get; set; }

        public virtual string TenantId { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>

        public virtual WorkflowVariable Variables { get; set; }
    }
}