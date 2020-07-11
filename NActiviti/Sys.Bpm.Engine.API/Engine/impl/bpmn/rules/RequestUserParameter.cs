using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <summary>
    /// 获取用户信息请求参数
    /// </summary>
    public class RequestUserParameter
    {
        /// <summary>
        /// 数据集主关键查询集合
        /// </summary>
        public string[] IdList { get; set; }

        /// <summary>
        /// 类别，目前包含
        /// </summary>
        public string Category { get; set; }
    }
}
