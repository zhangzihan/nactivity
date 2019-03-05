using org.activiti.engine;
using org.activiti.engine.repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace Sys.Bpmn.Test.engine.repository
{
    public class DeployBuilderTest
    {
        private UnitTestContext unitTestContext = UnitTestContext.CreateDefaultUnitTestContext();

        public DeployBuilderTest()
        {
        }

        /// <summary>
        /// 资源部署测试,使用两个测试用例.不检测重复,即使相同也会重复部署
        /// </summary>
        /// <param name="bpmnName"></param>
        [Theory]
        [InlineData("简单顺序流.bpmn")]
        [InlineData("条件分支流程.bpmn")]
        public void DeployBuilerTest_资源部署测试(string bpmnName)
        {
            Exception ex = Record.Exception(() =>
            {
                IRepositoryService repository = unitTestContext.Resolve<IProcessEngine>().RepositoryService;

                IDeploymentBuilder builder = repository.createDeployment();

                string bpmnXml = unitTestContext.ReadBpmn(bpmnName);

                IDeployment deployment = builder.name(Path.GetFileNameWithoutExtension(bpmnName))
                    .tenantId(unitTestContext.TenantId)
                    .startForm(null, bpmnXml)
                    .addString(bpmnName, bpmnXml)
                    .deploy();

                IDeploymentQuery dq = repository.createDeploymentQuery();
                deployment = dq.deploymentId(deployment.Id).singleResult();

                Assert.NotNull(deployment);
            });

            Assert.Null(ex);
        }
        
        /// <summary>
        /// 资源部署测试,使用两个测试用例.检测重复,如果相同就不会重复部署
        /// </summary>
        /// <param name="bpmnName"></param>
        [Theory]
        [InlineData("简单顺序流.bpmn")]
        [InlineData("条件分支流程.bpmn")]
        public void DeployBuilerTest_资源部署测试_启用重复性检测只有变更才会生成新版本(string bpmnName)
        {
            Exception ex = Record.Exception(() =>
            {
                string name = Path.GetFileNameWithoutExtension(bpmnName);

                IRepositoryService repository = unitTestContext.Resolve<IProcessEngine>().RepositoryService;

                IDeploymentQuery dq = repository.createDeploymentQuery();
                long count = dq.deploymentName(name).count();

                IDeploymentBuilder builder = repository.createDeployment();

                string bpmnXml = unitTestContext.ReadBpmn(bpmnName);

                IDeployment deployment = builder.name(name)
                    .tenantId(unitTestContext.TenantId)
                    .enableDuplicateFiltering()
                    .startForm(null, bpmnXml)
                    .addString(bpmnName, bpmnXml)
                    .deploy();

                long depCount = dq.deploymentName(name).count();

                Assert.True(count == 0 ? depCount > 0 : count == depCount);
            });

            Assert.Null(ex);
        }
    }
}
