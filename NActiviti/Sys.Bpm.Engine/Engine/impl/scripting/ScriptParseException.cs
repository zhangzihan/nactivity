using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sys.Workflow.Engine.Impl.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class ScriptParseException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public ScriptParseException(string expressText) : base($"脚本分析错误{expressText}")
        {
            ExpressText = expressText;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExpressText { get; }
    }
}
