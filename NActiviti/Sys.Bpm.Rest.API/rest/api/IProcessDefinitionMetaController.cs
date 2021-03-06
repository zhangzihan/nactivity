﻿using Sys.Workflow.Cloud.Services.Rest.Api.Resources;

namespace Sys.Workflow.Cloud.Services.Rest.Api
{
    /// <summary>
    /// 流程定义元数据RestAPI
    /// </summary>
    public interface IProcessDefinitionMetaController
    {
        /// <summary>
        /// 读取流程定义元数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProcessDefinitionMetaResource GetProcessDefinitionMetadata(string id);
    }
}