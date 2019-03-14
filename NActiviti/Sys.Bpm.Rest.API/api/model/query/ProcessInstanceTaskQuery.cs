using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{
    /// <summary>
    /// 流程实例化任务查询对象
    /// </summary>
    public class ProcessInstanceTaskQuery : AbstractQuery
    {
        /// <summary>
        /// 流程实例id
        /// </summary>
        public string ProcessInstanceId { get; set; }


        /// <summary>
        /// 包含已完成的实例
        /// </summary>
        public bool IncludeCompleted { get; set; } = true;
    }
}
