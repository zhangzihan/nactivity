using Newtonsoft.Json;
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

namespace org.activiti.cloud.services.api.commands
{
    /// <summary>
    /// 完成任务命令
    /// </summary>
    public class CompleteTaskCmd : ICommand
    {
        private readonly string id = "completeTaskCmd";
        private string taskId;
        private IDictionary<string, object> outputVariables;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="outputVariables">提交的数据</param>
        ////[JsonConstructor]
        public CompleteTaskCmd([JsonProperty("TaskId")] string taskId, [JsonProperty("OutputVariables")] IDictionary<string, object> outputVariables)
        {
            this.taskId = taskId;
            this.outputVariables = outputVariables;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get => id;
        }

        /// <summary>
        /// 提交的数据
        /// </summary>
        public virtual IDictionary<string, object> OutputVariables
        {
            get
            {
                outputVariables = outputVariables ?? new Dictionary<string, object>();
                return outputVariables;
            }
            set => outputVariables = value;
        }

        /// <summary>
        /// 任务id
        /// </summary>
        public virtual string TaskId
        {
            get => taskId;
            set => taskId = value;
        }
    }

}