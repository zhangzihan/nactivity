using Sys.Workflow.cloud.services.api.model;
using Sys.Workflow.cloud.services.rest.api;
using org.springframework.hateoas;
using Sys.Bpmn.Test;
using System;
using System.Linq;
using Xunit;

namespace Sys.Bpm.Client.Tests.rest.client
{
    //[Order(5)]
    public class ProcessInstanceTasksClientTest
    {
        private readonly IntegrationTestContext ctx = IntegrationTestContext.CreateDefaultUnitTestContext();

        public ProcessInstanceTasksClientTest()
        {
        }

        [Theory]
        [InlineData("简单顺序流.bpmn")]
        public void GetTasks_先启动一个流程_再查询流程任务记录数(string bpmnFile)
        {
            var ex = Record.Exception(() =>
            {
                string uid = Guid.NewGuid().ToString();
                ProcessInstance[] inst = AsyncHelper.RunSync(() => ctx.StartUseFile(bpmnFile, new string[] { uid }));

                ProcessInstanceTaskQuery query = new ProcessInstanceTaskQuery
                {
                    ProcessInstanceId = inst[0].Id,
                    IncludeCompleted = true
                };

                IProcessInstanceTasksController taskClient = ctx.CreateWorkflowHttpProxy().GetProcessInstanceTasksClient();

                Resources<TaskModel> list = AsyncHelper.RunSync<Resources<TaskModel>>(() => taskClient.GetTasks(query));

                Assert.NotNull(list);
                Assert.True(list.TotalCount == list.List.Count());
            });

            Assert.Null(ex);
        }
    }
}
