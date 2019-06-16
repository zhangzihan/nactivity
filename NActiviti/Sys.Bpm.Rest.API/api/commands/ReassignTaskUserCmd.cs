using Newtonsoft.Json;
using Sys.Workflow.Services.Api.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Cloud.Services.Api.Commands
{
    /// <summary>
    /// 重新指派流程节点执行人，该操作由管理员操作。该节点将终止当前所有待办任务.
    /// 重新由该节点处执行流程.
    /// </summary>
    public class ReassignTaskUserCmd : ICommand
    {
        public ReassignTaskUserCmd([JsonProperty]ReassignUser[] users)
        {
            this.Assignees = users;
        }

        public string Id { get; } = "reassignTaskUserCmd";

        /// <summary>
        /// 待分配指派人员列表
        /// </summary>
        public ReassignUser[] Assignees { get; set; }
    }
}
