using Newtonsoft.Json;
using org.activiti.services.api.commands;
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

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    /// 任务更新命令
    /// </summary>
    public class ApprovaleTaskCmd : ICommand
    {
        private readonly string id = "approvaleTaskCmd";

        public ApprovaleTaskCmd()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="comments">审批意见</param>
        /// <param name="variables">流程变量</param>
        //[JsonConstructor]
        public ApprovaleTaskCmd([JsonProperty("TaskId")] string taskId,
            [JsonProperty("Comments")] string comments,
            [JsonProperty("Variables")] WorkflowVariable variables)
        {
            this.TaskId = taskId;
            this.Comments = comments;
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
        /// 任务id
        /// </summary>
        public virtual string TaskId { get; set; }

        /// <summary>
        /// 审批意见
        /// </summary>
        public virtual string Comments { get; set; }

        /// <summary>
        /// 任务描述
        /// </summary>
        public virtual WorkflowVariable Variables { get; set; }
    }
}