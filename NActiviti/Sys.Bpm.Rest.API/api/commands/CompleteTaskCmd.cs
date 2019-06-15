using Newtonsoft.Json;
using org.activiti.engine.impl;
using org.activiti.services.api.commands;
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
        private WorkflowVariable outputVariables;

        /// <summary>
        /// 构造函数
        /// </summary>
        public CompleteTaskCmd()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskId">任务id</param>
        /// <param name="outputVariables">提交的数据</param>
        /// <param name="runtimeAssignUser">如果下一步需要从当前任务中指定人员处理,则使用这个参数</param>
        ////[JsonConstructor]
        public CompleteTaskCmd([JsonProperty("TaskId")] string taskId,
            [JsonProperty("OutputVariables")] WorkflowVariable outputVariables,
            [JsonProperty("RuntimeAssignUsers")]RuntimeAssigneeUser runtimeAssignUser)
        {
            this.taskId = taskId;
            this.outputVariables = outputVariables;
            this.RuntimeAssigneeUser = runtimeAssignUser;
        }

        /// <summary>
        /// 命令id
        /// </summary>
        public virtual string Id
        {
            get => id;
        }

        /// <summary>
        /// 任务关联的业务主键,需要和Assignee同时使用
        /// </summary>
        public virtual string BusinessKey
        {
            get; set;
        }

        /// <summary>
        /// 任务执行者,需要和BusinessKey同时使用
        /// </summary>
        public virtual string Assignee
        {
            get; set;
        }

        /// <summary>
        /// 任务附加备注
        /// </summary>
        public virtual string Comment
        {
            get; set;
        }

        /// <summary>
        /// 提交的数据
        /// </summary>
        public virtual WorkflowVariable OutputVariables
        {
            get
            {
                outputVariables = outputVariables ?? new WorkflowVariable();
                return outputVariables;
            }
            set
            {
                outputVariables = value;
            }
        }

        /// <summary>
        /// 任务id
        /// </summary>
        public virtual string TaskId
        {
            get => taskId;
            set => taskId = value;
        }

        public bool LocalScope
        {
            get; set;
        }

        /// <summary>
        /// 如果下一步需要从当前任务中指定人员处理,则使用这个参数
        /// </summary>
        public RuntimeAssigneeUser RuntimeAssigneeUser { get; set; }
    }
}