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
    /// 不允许转审
    /// </summary>
    [Serializable]
    public class NotSupportTransferException : ActivitiException
    {
        public NotSupportTransferException() : base("不允许转审")
        {
        }
    }
}