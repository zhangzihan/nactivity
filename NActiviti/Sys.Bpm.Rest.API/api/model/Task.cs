using System;
using Newtonsoft.Json;
using Sys.Net.Http;
using Sys.Workflow;

/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Sys.Workflow.cloud.services.api.model
{

    /// <summary>
    /// 流程任务
    /// </summary>
    public class TaskModel
    {

        /// <summary>
        /// 任务状态
        /// </summary>
        public enum TaskStatus
        {

            /// <summary>
            /// 已创建
            /// </summary>
            CREATED,

            /// <summary>
            /// 已分配
            /// </summary>
            ASSIGNED,

            /// <summary>
            /// 已挂起
            /// </summary>
            SUSPENDED,

            /// <summary>
            /// 已取消
            /// </summary>
            CANCELLED
        }

        private string id;
        private string owner;
        private string assignee;
        private string name;
        private string description;
        private DateTime? createdDate;
        private DateTime? endDate;
        private DateTime? claimedDate;
        private DateTime? dueDate;
        private int? priority;
        private string processDefinitionId;
        private string processInstanceId;
        private string parentTaskId;
        private string status;
        private string formKey;

        /// <summary>
        /// 
        /// </summary>
        public TaskModel()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="owner">任务所有者</param>
        /// <param name="assignee">分配人id</param>
        /// <param name="name">任务名称</param>
        /// <param name="description">任务描述</param>
        /// <param name="createdDate">提交日期</param>
        /// <param name="endDate">完成日期</param>
        /// <param name="claimedDate">接受日期</param>
        /// <param name="dueDate">过期日期</param>
        /// <param name="priority">任务优先级</param>
        /// <param name="processDefinitionId">流程定义id</param>
        /// <param name="processInstanceId">流程实例id</param>
        /// <param name="parentTaskId">父任务id</param>
        /// <param name="formKey">表单key</param>
        /// <param name="status">任务状态</param>
        /// <param name="deleteReason">任务取消原因</param>
        /// <param name="isTransfer">是否是转派任务</param>
        /// <param name="canTransfer">是否允许转派</param>
        /// <param name="onlyAssignee">仅允许一人执行该任务</param>
        //[JsonConstructor]
        public TaskModel([JsonProperty("Id")]string id,
            [JsonProperty("Owner")]string owner,
            [JsonProperty("Assignee")]string assignee,
            [JsonProperty("name")]string name,
            [JsonProperty("Description")]string description,
            [JsonProperty("CreatedDate")]DateTime? createdDate,
            [JsonProperty("EndDate")]DateTime? endDate,
            [JsonProperty("ClaimedDate")]DateTime? claimedDate,
            [JsonProperty("DueDate")]DateTime? dueDate,
            [JsonProperty("Priority")]int? priority,
            [JsonProperty("ProcessDefinitionId")]string processDefinitionId,
            [JsonProperty("ProcessInstanceId")]string processInstanceId,
            [JsonProperty("ParentTaskId")]string parentTaskId,
            [JsonProperty("FormKey")]string formKey,
            [JsonProperty("Status")]string status,
            [JsonProperty("DeleteReason")]string deleteReason,
            [JsonProperty("isTransfer")]bool? isTransfer,
            [JsonProperty("CanTransfer")]bool? canTransfer,
            [JsonProperty("OnlyAssignee")]bool? onlyAssignee,
            [JsonProperty("BusinessKey")]string businessKey)
        {
            this.id = id;
            this.owner = owner;
            this.assignee = assignee;
            this.name = name;
            this.description = description;
            this.createdDate = createdDate;
            this.endDate = endDate;
            this.claimedDate = claimedDate;
            this.dueDate = dueDate;
            this.priority = priority;
            this.processDefinitionId = processDefinitionId;
            this.processInstanceId = processInstanceId;
            this.parentTaskId = parentTaskId;
            this.formKey = formKey;
            this.status = status;
            this.DeleteReason = deleteReason;
            this.IsTransfer = isTransfer;
            this.CanTransfer = canTransfer;
            this.OnlyAssignee = onlyAssignee;
            this.BusinessKey = businessKey;
        }


        /// <summary>
        /// id
        /// </summary>
        public virtual string Id
        {
            get => id;
            set => id = value;
        }

        public virtual string BusinessKey
        {
            get; set;
        }

        /// <summary>
        /// 任务所有者
        /// </summary>

        public virtual string Owner
        {
            get => owner;
            set => owner = value;
        }


        /// <summary>
        /// 分配人id
        /// </summary>
        public virtual string Assignee
        {
            get => assignee;
            set => assignee = value;
        }

        /// <summary>
        /// 任务名称
        /// </summary>

        public virtual string Name
        {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// 任务描述
        /// </summary>
        public virtual string Description
        {
            get => description;
            set => description = value;
        }

        /// <summary>
        /// 提交日期
        /// </summary>
        public virtual DateTime? CreatedDate
        {
            get => createdDate;
            set => createdDate = value;
        }

        /// <summary>
        /// 完成日期
        /// </summary>
        public virtual DateTime? EndDate
        {
            get => endDate;
            set => endDate = value;
        }


        /// <summary>
        /// 接受日期
        /// </summary>
        public virtual DateTime? ClaimedDate
        {
            get => claimedDate;
            set => claimedDate = value;
        }


        /// <summary>
        /// 过期日期
        /// </summary>
        public virtual DateTime? DueDate
        {
            get => dueDate;
            set => dueDate = value;
        }


        /// <summary>
        /// 任务优先级
        /// </summary>
        public virtual int? Priority
        {
            get => priority;
            set => priority = value;
        }


        /// <summary>
        /// 流程定义id
        /// </summary>
        public virtual string ProcessDefinitionId
        {
            get => processDefinitionId;
            set => processDefinitionId = value;
        }


        /// <summary>
        /// 流程实例id
        /// </summary>
        public virtual string ProcessInstanceId
        {
            get => processInstanceId;
            set => processInstanceId = value;
        }


        /// <summary>
        /// 任务状态
        /// </summary>
        public virtual string Status
        {
            get => status;
            set => status = value;
        }


        /// <summary>
        /// 父任务id
        /// </summary>
        public virtual string ParentTaskId
        {
            get => parentTaskId;
            set => parentTaskId = value;
        }

        /// <summary>
        ///表单key
        /// </summary>
        public virtual string FormKey
        {
            get => formKey;
            set => formKey = value;
        }

        /// <summary>
        /// 是否转派任务
        /// </summary>
        public bool? IsTransfer { get; set; }

        /// <summary>
        /// 是否允许转派
        /// </summary>
        public bool? CanTransfer { get; set; }

        /// <summary>
        /// 只能单人执行该任务
        /// </summary>
        public bool? OnlyAssignee { get; set; }

        /// <summary>
        /// 执行人信息
        /// </summary>
        public UserInfo Assigner
        {
            get; set;
        }

        /// <summary>
        /// 任务取消原因
        /// </summary>
        public virtual string DeleteReason
        {
            get; set;
        }
    }
}