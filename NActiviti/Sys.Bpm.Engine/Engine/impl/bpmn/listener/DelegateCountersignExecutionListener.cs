﻿///////////////////////////////////////////////////////////
//  GetBookmarkRuleProvider.cs
//  Implementation of the Class GetBookmarkRuleProvider
//  Generated by Enterprise Architect
//  Created on:      30-1月-2019 8:32:00
//  Original author: 张楠
///////////////////////////////////////////////////////////

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sys.Workflow.Bpmn.Constants;
using Sys.Workflow.Bpmn.Models;
using Sys.Workflow.Engine.Delegate;
using Sys.Workflow.Engine.Impl.Contexts;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys;
using Sys.Workflow;
using Sys.Workflow.Engine.Bpmn.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sys.Expressions;
using Sys.Workflow.Engine.Impl.Identities;
using Sys.Workflow.Engine.Delegate.Events;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Engine.Impl.Cfg;

namespace Sys.Workflow.Engine.Impl.Bpmn.Listeners
{
    /// <summary>
    /// 会签任务监听器,侦听节点任务开始事件,然后根据会签策略从业务服务拉取相关的人员。
    /// 会签角色是指当前节点执行人,根据节点设置规则获取执行人.
    /// 1.发起者规则,指当前节点执行人.策略名:GetExecutor.
    /// 2.用户规则,指系统内的用户.策略名:GetUser.
    /// 3.岗位规则,对系统内的登录用户或组织人员按照岗位职责分类的一种方式.
    /// 该规则将读取指定岗位下的所有用户.策略名:GetDuty.
    /// 3.部门规则,读取指定部门下的所有用户.策略名:GetDept.
    /// 4.发起者下属,指组织人员行政上下级的归属关系,读取该发起者所有的下属人员.策略名:GetUnderling.
    /// 5.发起者直接汇报对象,上下级结构不在已行政关系为主,而是根据岗位职责形成的一种上下级汇报关系.
    /// 比如组建项目团队时,团队内的成员在项目进行期内的上级领导归属为项目负责人,而该负责人并不一定和干系人存在行政上的上下级关系.策略名:GetDirectReporter.
    /// 6.发起者部门领导,指组织人员行政上下级的归属关系,读取发起者的部门领导.策略名:GetDeptLeader.
    /// [{"ruleType":"GetUser","ruleName":"GetUser","queryCondition":[{"id":"用户1","name":"用户1"}]},{"ruleType":"GetDept","ruleName":"GetDept","queryCondition":[{"id":"部门1","name":"部门1"}]}{"ruleType":"GetDeptLeader","ruleName":"GetDeptLeader","queryCondition":[{"id":"部门1","name":"部门1"}]},{"ruleType":"GetDirectReporter","ruleName":"GetDirectReporter","queryCondition":[{"id":"部门1","name":"部门1"}]},{"ruleType":"GetDuty","ruleName":"GetDuty","queryCondition":[{"id":"岗位1","name":"岗位1"}]},{"ruleType":"GetUnderling","ruleName":"GetUnderling","queryCondition":[{"id":"领导1","name":"领导1"}]}]
    /// </summary>
    public class DelegateCountersignExecutionListener : IExecutionListener, IActivitiEventListener
    {
        /// <summary>
        /// 
        /// </summary>
        public DelegateCountersignExecutionListener()
        {

        }

        public bool FailOnException => true;

        /// <summary>
        /// 侦听接收通知处理
        /// </summary>
        /// <param name="execution"></param>
        public void Notify(IExecutionEntity execution)
        {
            IUserDelegateAssignProxy userDelegateAssignProxy = ProcessEngineServiceProvider.Resolve<IUserDelegateAssignProxy>();

            userDelegateAssignProxy.Assign(execution);
        }

        public void OnEvent(IActivitiEvent @event)
        {
            if (@event is IActivitiSequenceFlowTakenEvent entity
                && entity.ExecutionId is not null)
            {
                var targetElement = ProcessDefinitionUtil.GetFlowElement(entity.ProcessDefinitionId, entity.TargetActivityId);
                if (targetElement is UserTask)
                {
                    var pec = Context.ProcessEngineConfiguration ?? ProcessEngineServiceProvider.Resolve<ProcessEngineConfiguration>() as ProcessEngineConfigurationImpl;
                    IExecutionEntity execution = pec.ExecutionEntityManager.FindById<IExecutionEntity>(entity.ExecutionId);
                    Notify(execution);
                }
            }
        }
    }

    class DefaultUserDelegateAssignProxy : IUserDelegateAssignProxy
    {
        private static readonly Regex EXPR_PATTERN = new Regex(@"\$\{(.*?)\}", RegexOptions.Multiline);

        private static readonly ILogger<DefaultUserDelegateAssignProxy> logger = ProcessEngineServiceProvider.LoggerService<DefaultUserDelegateAssignProxy>();

        public DefaultUserDelegateAssignProxy()
        {

        }

        public void Assign(IExecutionEntity execution)
        {
            UserTask userTask = null;
            switch (execution.CurrentFlowElement)
            {
                case SequenceFlow flowNode:
                    if (flowNode.TargetFlowElement is UserTask)
                    {
                        userTask = flowNode.TargetFlowElement as UserTask;
                    }
                    break;
                case UserTask _:
                    userTask = execution.CurrentFlowElement as UserTask;
                    break;
            }

            if (userTask is not null && userTask.HasMultiInstanceLoopCharacteristics())
            {
                var varName = userTask.LoopCharacteristics.GetCollectionVarName();

                if (execution.GetVariable(varName) is null)
                {
                    List<IUserInfo> users = new List<IUserInfo>();
                    string getUserPolicy = userTask.GetUsersPolicy();
                    if (string.IsNullOrWhiteSpace(getUserPolicy) == false)
                    {
                        if (EXPR_PATTERN.IsMatch(getUserPolicy))
                        {
                            getUserPolicy = EXPR_PATTERN.Replace(getUserPolicy, (m) =>
                            {
                                var value = m.Groups[1].Value;
                                var variables = execution.Variables;
                                object roles = ExpressionManager.GetValue(variables, value, variables);
                                return roles.ToString();
                            });
                        }

                        QueryBookmark[] actors = JsonConvert.DeserializeObject<QueryBookmark[]>(getUserPolicy);

                        IGetBookmarkRuleProvider ruleProvider = ProcessEngineServiceProvider.Resolve<IGetBookmarkRuleProvider>();

                        foreach (QueryBookmark query in actors)
                        {
                            IGetBookmarkRule rule = ruleProvider.CreateBookmarkRule(query.RuleType.ToString());
                            rule.Execution = execution;
                            rule.Condition = query;
                            var us = Context.ProcessEngineConfiguration.CommandExecutor.Execute(rule);
                            users.AddRange(us);
                        }
                    }

                    if (users.Count == 0)
                    {
                        users.Add(Authentication.AuthenticatedUser);
                        logger.LogWarning($"调用查询人员服务失败,分组没有人,重新指到当前提交人,Duty={getUserPolicy}");
                        //throw new NoneCountersignUsersException(execution.CurrentFlowElement.Name);
                    }

                    execution.SetVariable(varName, users.Count == 0 ? new string[] { "" } : users.Select(x => x.Id).Distinct().ToArray());
                }
            }
        }
    }
}
