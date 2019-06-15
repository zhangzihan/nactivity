using Microsoft.Extensions.DependencyInjection;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessEngineBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        IServiceCollection Services { get; }
    }
}
