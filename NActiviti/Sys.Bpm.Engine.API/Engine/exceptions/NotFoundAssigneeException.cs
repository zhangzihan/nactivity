///////////////////////////////////////////////////////////
//  AddContersignCmd.cs
//  Implementation of the Class AddContersignCmd
//  Created on:      1-2月-2019 8:32:00
//  Original author: 张楠

using System;
using System.Runtime.Serialization;

namespace org.activiti.engine.exceptions
{
    /// <summary>
    /// 找不到待分配的人员
    /// </summary>
    [Serializable]
    public class NotFoundAssigneeException : ActivitiException
    {
        public NotFoundAssigneeException(string assignee = "") : base($"找不到{assignee}待分配的人员")
        {
        }
    }
}