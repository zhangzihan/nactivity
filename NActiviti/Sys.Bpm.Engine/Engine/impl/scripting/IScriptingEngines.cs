using Sys.Workflow.Engine.Impl.Persistence.Entity;

namespace Sys.Workflow.Engine.Impl.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public interface IScriptingEngines
    {
        /// <summary>
        /// 
        /// </summary>
        bool CacheScriptingEngines { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="script"></param>
        /// <param name="execution"></param>
        /// <returns></returns>
        object Evaluate(string script, IExecutionEntity execution);
    }
}