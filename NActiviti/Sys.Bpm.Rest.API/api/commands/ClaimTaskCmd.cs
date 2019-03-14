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

using Newtonsoft.Json;
using System;

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    ///任务领取命令
    /// </summary>
    public class ClaimTaskCmd : ICommand
    {
        private readonly string id = "claimTaskCmd";
        private string taskId;
        private string assignee;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="assignee">领取人id</param>
        ////[JsonConstructor]
        public ClaimTaskCmd([JsonProperty("TaskId")]string taskId, 
            [JsonProperty("Assignee")]string assignee)
        {
            this.taskId = taskId;
            this.assignee = assignee;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get
            {
                return id;
            }
        }

        /// <summary>
        /// 任务id
        /// </summary>
        public virtual string TaskId
        {
            get
            {
                return taskId;
            }
            set => taskId = value;
        }

        /// <summary>
        /// 领取人id
        /// </summary>
        public virtual string Assignee
        {
            get
            {
                return assignee;
            }
            set => assignee = value;
        }
    }

}