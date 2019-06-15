using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using org.activiti.engine;
using org.activiti.engine.impl;
using org.activiti.engine.impl.agenda;
using org.activiti.engine.impl.asyncexecutor;
using org.activiti.engine.impl.cfg;
using org.activiti.validation;
using Sys.Bpm.Model;
using Sys.Workflow.Engine.Bpmn.Rules;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public static class ProcessEngineBuilderExtensionsProcessEngine
    {
        /// <summary>
        ///  注入流程引擎配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngineConfiguration(this IProcessEngineBuilder builder)
        {
            builder.Services.AddSingleton<ProcessEngineConfiguration>(sp =>
            {
                return new StandaloneProcessEngineConfiguration(
                    new HistoryServiceImpl(),
                    new TaskServiceImpl(),
                    new DynamicBpmnServiceImpl(),
                    new RepositoryServiceImpl(),
                    new RuntimeServiceImpl(),
                    new ManagementServiceImpl(),
                    //sp.GetService<IAsyncExecutor>(),
                    null,
                    sp.GetService<IConfiguration>()
                );
            });

            return builder;
        }

        /// <summary>
        /// 注入流程引擎服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngineService(this IProcessEngineBuilder builder)
        {
            builder.AddBookmarkRuleProvider();

            builder.Services.AddTransient<IActivitiEngineAgendaFactory>(sp => new DefaultActivitiEngineAgendaFactory());

            builder.Services.AddSingleton<IBpmnParseFactory, DefaultBpmnParseFactory>();

            builder.AddProcessValidator();

            builder.Services.AddTransient<IRepositoryService>(sp => sp.GetRequiredService<IProcessEngine>().RepositoryService);

            builder.Services.AddTransient<IRuntimeService>(sp => sp.GetRequiredService<IProcessEngine>().RuntimeService);

            builder.Services.AddTransient<IManagementService>(sp => sp.GetRequiredService<IProcessEngine>().ManagementService);

            builder.Services.AddTransient<IHistoryService>(sp => sp.GetRequiredService<IProcessEngine>().HistoryService);

            builder.Services.AddTransient<ITaskService>(sp => sp.GetRequiredService<IProcessEngine>().TaskService);

            builder.Services.AddTransient<IDynamicBpmnService>(sp => sp.GetRequiredService<IProcessEngine>().DynamicBpmnService);

            builder.Services.AddTransient<IProcessEngine>(sp =>
            {
                return sp.GetService<ProcessEngineFactory>().DefaultProcessEngine;
            });

            builder.Services.AddBpmModelServiceProvider();

            return builder;
        }

        /// <summary>
        /// 注入会签人员提供类
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddBookmarkRuleProvider(this IProcessEngineBuilder builder)
        {
            IGetBookmarkRuleProvider getBookmarkRuleProvider = new GetBookmarkRuleProvider();

            builder.Services.AddSingleton(getBookmarkRuleProvider);

            return builder;
        }

        /// <summary>
        /// 注入流程引擎工厂
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IProcessEngineBuilder AddProcessEngineFactory(this IProcessEngineBuilder builder)
        {
            builder.Services.AddSingleton<ProcessEngineFactory>(sp =>
            {
                return ProcessEngineFactory.Instance;
            });

            return builder;
        }

        /// <summary>
        /// 注入流程模型验证
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static IProcessEngineBuilder AddProcessValidator(this IProcessEngineBuilder builder)
        {
            IProcessValidator processValidator = new ProcessValidatorFactory().CreateProcessValidator();

            builder.Services.AddSingleton(processValidator);

            return builder;
        }
    }
}
