namespace Sys.Workflow.Engine.Impl.Scripting
{
    /// <summary>
    /// 
    /// </summary>
    public interface IScriptingEnginesProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public IScriptingEngines Create(string lang)
        {
            return ProcessEngineServiceProvider.Resolve<IScriptingEngines>();
        }
    }
}