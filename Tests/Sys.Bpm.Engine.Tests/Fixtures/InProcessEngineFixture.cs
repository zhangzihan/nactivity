using Sys.Workflow;
using Sys.Workflow.Engine;
using Sys.Workflow.Engine.Impl;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Impl.Interceptor;
using System;
using System.IO;
using Xunit;

namespace Sys.Bpm.Engine.Tests.Fixtures
{
    /// <summary>
    /// 引擎测试集合定义
    /// </summary>
    [CollectionDefinition("EngineTestCollection")]
    public class EngineTestCollection : ICollectionFixture<InProcessEngineFixture>
    {
    }

    /// <summary>
    /// 进程内引擎Fixture，为所有测试共享同一个引擎实例
    /// </summary>
    public class InProcessEngineFixture : IDisposable
    {
        private readonly InProcessWorkflowEngine context;

        public IProcessEngine ProcessEngine { get; }

        public ICommandExecutor CommandExecutor { get; }

        public string TenantId => context.TenantId;

        public InProcessEngineFixture()
        {
            context = new InProcessWorkflowEngine();
            context.Create();

            ProcessEngineConfiguration processEngineConfig = new StandaloneProcessEngineConfiguration(
                    new HistoryServiceImpl(),
                    new TaskServiceImpl(),
                    new DynamicBpmnServiceImpl(),
                    new RepositoryServiceImpl(),
                    new RuntimeServiceImpl(),
                    new ManagementServiceImpl(),
                    null,
                    ProcessEngineServiceProvider.Resolve<Microsoft.Extensions.Configuration.IConfiguration>())
            {
                ProcessEngineName = "InProcessEngineTests",
                AsyncExecutorActivate = false,
                DatabaseSchemaUpdate = "false",
                EnableVerboseExecutionTreeLogging = false,
                History = "audit"
            };

            ProcessEngine = ProcessEngineFactory.CreateProcessEngine(processEngineConfig);
            CommandExecutor = (ProcessEngine.ProcessEngineConfiguration as ProcessEngineConfigurationImpl).CommandExecutor;
        }

        /// <summary>
        /// 设置认证用户
        /// </summary>
        public void SetAuthenticatedUser()
        {
            Authentication.AuthenticatedUser = new InProcessWorkflowEngine.TestUser()
            {
                Id = "评审员",
                FullName = "评审员",
                TenantId = TenantId
            };
        }

        /// <summary>
        /// 读取BPMN文件
        /// </summary>
        /// <param name="name">文件名</param>
        /// <returns>BPMN XML内容</returns>
        public static string ReadBpmn(string name)
        {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            return File.ReadAllText(Path.Combine(root, "resources", "samples", name));
        }

        public void Dispose()
        {
            ProcessEngineFactory.Destroy();
        }
    }
}
