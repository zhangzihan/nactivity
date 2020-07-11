using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <summary>
    /// 获取用户信息类别
    /// </summary>
    public class RequestUserCategory
    {
        /// <summary>
        /// 获取部门下人员
        /// </summary>
        public const string GETUSERS_DEPT = "GetDept";
        /// <summary>
        /// 获取部门leader
        /// </summary>
        public const string GETUSER_DEPTLEADER = "GetDeptLeader";
        /// <summary>
        /// 获取直接汇报对象
        /// </summary>
        public const string GETUSER_DIRECTREPOT = "GetDirectReporter";
        /// <summary>
        /// 获取岗位下人员
        /// </summary>
        public const string GETUSERS_DUTY = "GetDuty";
        /// <summary>
        /// 获取流程执行人信息
        /// </summary>
        public const string GETUSER_EXECUTOR = "GetExecutor";
        /// <summary>
        /// 获取下属人员
        /// </summary>
        public const string GETUSERS_UNDERLING = "GetUnderling";
        /// <summary>
        /// 获取人员
        /// </summary>
        public const string GETUSER_USER = "GetUser";
        /// <summary>
        /// 
        /// </summary>
        public const string GETUSERS_OWNERDEPT = "GetOwnerDepartment";
    }
}
