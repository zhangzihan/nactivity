using System;

namespace Sys.Workflow.Engine.Impl.EL
{
    /// <summary>
    /// 
    /// </summary>
    public interface IValueExpression
    {
        /// <summary>
        /// 
        /// </summary>
        string ExpressionString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Type ValueType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elContext"></param>
        /// <returns></returns>
        object GetValue(ELContext elContext);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elContext"></param>
        /// <param name="value"></param>
        void SetValue(ELContext elContext, object value);
    }
}