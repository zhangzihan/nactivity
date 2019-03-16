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

namespace org.activiti.cloud.services.api.commands
{


    /// <summary>
    /// 任务更新命令
    /// </summary>
    public class TerminateTaskCmd : ICommand
    {
        private readonly string id = "terminateTaskCmd";

        /// <summary>
        /// 构造函数
        /// </summary>
        public TerminateTaskCmd()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="terminateReason">终止原因</param>
        //[JsonConstructor]
        public TerminateTaskCmd([JsonProperty("TaskId")] string taskId,
            [JsonProperty("Description")] string terminateReason) : this()
        {
            this.TaskId = taskId;
            this.TerminateReason = terminateReason;
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
            get;
            set;
        }

        /// <summary>
        /// 任务终止原因
        /// </summary>

        public virtual string TerminateReason
        {
            get;
            set;
        }
    }

}