using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.rest.api
{
    public static class WorkflowConstants
    {
        /// <summary>
        /// 流程定义路由基地址
        /// </summary>
        public const string PROC_DEF_ROUTER_V1 = "v1/workflow/process-definitions";

        /// <summary>
        /// 流程部署路由基地址
        /// </summary>
        public const string PROC_DEP_ROUTER_V1 = "v1/workflow/process-deployer";

        /// <summary>
        /// 流程实例路由基地址
        /// </summary>
        public const string PROC_INS_ROUTER_V1 = "v1/workflow/process-instances";

        /// <summary>
        /// 流程实例变量路由基地址
        /// </summary>
        public const string PROC_INS_VAR_ROUTER_V1 = "v1/workflow/process-instances/{processInstanceId}/variables";

        /// <summary>
        /// 流程任务路由基地址
        /// </summary>
        public const string TASK_ROUTER_V1 = "v1/workflow/tasks";

        /// <summary>
        /// 流程任务变量路由基地址
        /// </summary>
        public const string TASK_VAR_ROUTER_V1 = "v1/workflow/tasks/{taskId}/variables";
    }
}
