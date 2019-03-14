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
    /// 任务释放命令
    /// </summary>
    public class ReleaseTaskCmd : ICommand
    {

        private readonly string id = "releaseTaskCmd";
        private string taskId;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        ////[JsonConstructor]
        public ReleaseTaskCmd([JsonProperty("TaskId")] string taskId)
        {
            this.taskId = taskId;
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
        }
    }

}