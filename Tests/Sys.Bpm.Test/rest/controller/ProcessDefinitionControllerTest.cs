using org.activiti.api.runtime.shared.query;
using org.activiti.cloud.services.api.model;
using org.activiti.cloud.services.api.model.converter;
using org.activiti.cloud.services.core;
using org.activiti.cloud.services.core.pageable;
using org.activiti.cloud.services.rest.api;
using org.activiti.cloud.services.rest.api.resources;
using org.activiti.cloud.services.rest.assemblers;
using org.activiti.cloud.services.rest.controllers;
using org.activiti.engine;
using org.activiti.engine.repository;
using org.springframework.hateoas;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static org.activiti.api.runtime.shared.query.Sort;

namespace Sys.Bpmn.Test.rest.controller
{
    public class ProcessDefinitionControllerTest
    {
        private readonly UnitTestContext ctx = UnitTestContext.CreateDefaultUnitTestContext();

        public ProcessDefinitionControllerTest()
        {
        }

        private IProcessDefinitionController CreateController()
        {
            return new ProcessDefinitionControllerImpl(
                    ctx.Resolve<IProcessEngine>(),
                    ctx.Resolve<ProcessDefinitionConverter>(),
                    ctx.Resolve<ProcessDefinitionResourceAssembler>(),
                    ctx.Resolve<PageableProcessDefinitionRepositoryService>(),
                    ctx.Resolve<SecurityPoliciesApplicationService>());
        }

        [Theory]
        [InlineData(1, 20)]
        public void GetLatestProcessDefinitions_测试最终发布的流程定义(int offset, int pageSize)
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                IProcessDefinitionController pdc = CreateController();

                Resources<ProcessDefinition> result = pdc.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    TenantId = ctx.TenantId,
                    Pageable = new Pageable
                    {
                        PageNo = offset,
                        PageSize = pageSize,
                        Sort = new Sort(new Order[]
                        {
                            new Order(){ Direction = Direction.ASC, Property = "name" }
                        })
                    }
                }).Result;

                Assert.NotNull(result);
                Assert.True(result.RecordCount <= pageSize);
            }));

            Assert.Null(ex);
        }

        [Fact]
        public void GetProcessDefinition_根据ID查找流程定义()
        {
            Exception ex = Record.Exception((Action)(() =>
            {
                IProcessDefinitionController pdc = CreateController();

                Deployment dep = ctx.DeployTestProcess();

                ProcessDefinition pdr = pdc.LatestProcessDefinitions(new ProcessDefinitionQuery
                {
                    Name = dep.Name,
                    TenantId = ctx.TenantId,
                    Pageable = new Pageable
                    {
                        PageNo = 1,
                        PageSize = 1
                    }
                }).Result.List?.FirstOrDefault();

                ProcessDefinition find = pdc.GetProcessDefinition(pdr.Id).Result;

                Assert.NotNull(find);
                Assert.True(find.Id == pdr.Id);
            }));

            Assert.Null(ex);
        }
    }
}
