using Newtonsoft.Json;
using org.activiti.engine.history;
using System;
using System.Collections.Generic;

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

namespace org.activiti.cloud.services.api.model
{

    /// <summary>
    /// 流程实例
    /// </summary>
    public class HistoricInstance
    {

        /// <summary>
        /// 流程实例状态
        /// </summary>
        public enum HistoricInstanceStatus
        {
            /// <summary>
            /// 已完成
            /// </summary>
            COMPLETED,

            /// <summary>
            /// 已终止
            /// </summary>
            DELETED
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
        /// 完成日期
        /// </summary>
        public DateTime? EndDate
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

        /// <summary>
        /// 流程版本
        /// </summary>
        public int? ProcessDefinitionVersion
        {
            get; set;
        }

        /// <summary>
        /// 持续时间毫秒
        /// </summary>
        public long? DurationInMillis { get; set; }


        /// <summary>
        /// 提交人id
        /// </summary>
        public string StartUserId { get; set; }

        /// <summary>
        /// 删除原因
        /// </summary>
        public string DeleteReason { get; set; }

        /// <summary>
        ///租户id
        /// </summary>
        public string TenantId { get; set; }
    }
}