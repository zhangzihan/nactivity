using Sys.Workflow.Rules;
using Sys.Rules.DmnEngine;
using Sys.Rules.DmnEngine.Adapter;
using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 定义 DMN 决策运行时的依赖注入扩展。
    /// </summary>
    public static class DmnDecisionRuntimeServiceCollectionExtensions
    {
        /// <summary>
        /// 注册基于 Sys.Rules.DmnEngine 的决策执行服务。
        /// </summary>
        /// <param name="services">服务集合。</param>
        /// <param name="definitionSearchDirectories">文件系统定义搜索目录。</param>
        /// <returns>服务集合。</returns>
        public static IServiceCollection AddDmnDecisionExecutionService(this IServiceCollection services, IEnumerable<string>? definitionSearchDirectories = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<DmnEngineFacade>(sp =>
            {
                var decisionVersionRepository = sp.GetService<IDecisionVersionRepository>();
                var decisionExecutionLogRepository = sp.GetService<IDecisionExecutionLogRepository>();

                return decisionVersionRepository != null
                    ? new DmnEngineFacade(decisionVersionRepository, decisionExecutionLogRepository)
                    : new DmnEngineFacade(definitionSearchDirectories, decisionExecutionLogRepository);
            });
            services.AddSingleton<IDecisionExecutionService>(sp => new DmnDecisionExecutionService(
                sp.GetRequiredService<DmnEngineFacade>(),
                sp.GetService<IDecisionExecutionLogRepository>()));

            return services;
        }
    }
}
