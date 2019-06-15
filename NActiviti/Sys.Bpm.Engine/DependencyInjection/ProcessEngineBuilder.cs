using Microsoft.Extensions.DependencyInjection;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public class ProcessEngineBuilder : IProcessEngineBuilder
    {
        public IServiceCollection Services { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        public ProcessEngineBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
