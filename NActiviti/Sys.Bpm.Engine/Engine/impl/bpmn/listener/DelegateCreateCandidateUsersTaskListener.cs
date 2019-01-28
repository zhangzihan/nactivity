using org.activiti.engine.@delegate;
using org.activiti.engine.impl.persistence.entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.engine.impl.bpmn.listener
{
    public class DelegateCreateCandidateUsersTaskListener : ITaskListener
    {
        public void notify(IDelegateTask delegateTask)
        {
            List<string> assigneeList = new List<string>();
            assigneeList.Add("新用户1");
            assigneeList.Add("会签2");
            delegateTask.addCandidateUsers(assigneeList);
            //delegateTask.setVariable("userList", assigneeList);
        }
    }
}
