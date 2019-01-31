using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Engine.Bpmn.Rules
{
    /// <summary>
    /// 会签规则角色获取属性描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GetBookmarkDescriptorAttribute : Attribute
    {
        public GetBookmarkDescriptorAttribute(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// 规则名称
        /// </summary>
        public string Name { get; }
    }
}
