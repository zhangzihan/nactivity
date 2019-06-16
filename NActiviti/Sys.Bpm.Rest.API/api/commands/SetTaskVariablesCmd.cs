using Newtonsoft.Json;
using Sys.Workflow.services.api.commands;
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

namespace Sys.Workflow.cloud.services.api.commands
{

    /// <summary>
    /// 设置任务变量命令
    /// </summary>
    public class SetTaskVariablesCmd : ICommand
    {
        private readonly string id = "setTaskVariablesCmd";
        private string taskId;
        private WorkflowVariable variables;


        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="variables">变量列表</param>
        ////[JsonConstructor]
        public SetTaskVariablesCmd([JsonProperty("TaskId")] string taskId, [JsonProperty("Variables")] WorkflowVariable variables)
        {
            this.taskId = taskId;
            this.variables = variables;
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
            get
            {
                return taskId;
            }
            set => taskId = value;
        }

        /// <summary>
        /// 任务变量列表
        /// </summary>

        public virtual WorkflowVariable Variables
        {
            get
            {
                return variables;
            }
            set => variables = value;
        }
    }

}