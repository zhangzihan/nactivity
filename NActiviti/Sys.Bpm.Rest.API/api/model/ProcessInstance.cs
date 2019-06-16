using Newtonsoft.Json;
using Sys.Net.Http;
using Sys.Workflow;
using System;

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

namespace Sys.Workflow.Cloud.Services.Api.Model
{

    /// <summary>
    /// 流程实例
    /// </summary>
    public class ProcessInstance
    {

        /// <summary>
        /// 流程实例状态
        /// </summary>
        public enum ProcessInstanceStatus
        {

            /// <summary>
            /// 运行中
            /// </summary>
            RUNNING,

            /// <summary>
            /// 已挂起
            /// </summary>
            SUSPENDED,

            /// <summary>
            /// 已完成
            /// </summary>
            COMPLETED
        }


        /// <summary>
        /// 构造函数
        /// </summary>
        public ProcessInstance()
        {
        }

        /// <summary>
        /// id
        /// </summary>

        public virtual string Id
        {
            get; set;
        }

        /// <summary>
        /// 名称
        /// </summary>

        public virtual string Name
        {
            get; set;
        }

        /// <summary>
        /// 描述
        /// </summary>

        public virtual string Description
        {
            get; set;
        }

        /// <summary>
        /// 开始日期
        /// </summary>

        public virtual DateTime? StartDate
        {
            get; set;
        }

        /// <summary>
        /// 提交人id
        /// </summary>

        public virtual string StartUserId
        {
            get; set;
        }

        /// <summary>
        /// 提交人
        /// </summary>
        public virtual UserInfo Starter
        {
            get; set;
        }


        /// <summary>
        /// 业务键值
        /// </summary>
        public virtual string BusinessKey
        {
            get; set;
        }

        /// <summary>
        /// 状态
        /// </summary>

        public virtual string Status
        {
            get; set;
        }

        /// <summary>
        /// 流程定义id
        /// </summary>

        public virtual string ProcessDefinitionId
        {
            get; set;
        }

        /// <summary>
        /// 流程键值
        /// </summary>

        public virtual string ProcessDefinitionKey
        {
            get; set;
        }
    }
}