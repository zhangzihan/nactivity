using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.bpmn.constants
{
    /// <summary>
    ///  多人员处理同一个用户任务时的处理方式，
    ///  single：仅单人执行
    ///  all：所有人都执行完成
    ///  one：一人执行完成即可
    /// </summary>
    public class AssigneeType
    {
        public const string SINGLE = "single";
        public const string ALL = "all";
        public const string ONE = "one";
        public const string HALF_PASSED = "halfpassed";
        public const string HALF_REJECT = "halfreject";
    }
}
