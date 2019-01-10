using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Bpm.api.model
{
    public class ProcessDefinitionDeployer
    {
        /// <summary>
        /// 是否启用BPMN2.0模式验证
        /// </summary>
        public bool DisableSchemaValidation { get; set; } = true;

        /// <summary>
        /// 是否验证流程
        /// </summary>
        public bool DisableBpmnValidation { get; set; } = true;

        /// <summary>
        /// 模型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模型XML schema
        /// </summary>
        public string Category { get; set; } = "http://camunda.org/schema/1.0/bpmn20";

        /// <summary>
        /// 模型唯一标识
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 模型所属租户id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 如果启用，此部署将与以前的任何部署进行比较。
        /// 这意味着每个(非生成的)资源都将与此部署提供的资源进行比较
        /// </summary>
        public bool EnableDuplicateFiltering { get; set; } = true;

        /// <summary>
        /// BPMN2.0模型定义XML字符串
        /// </summary>
        public string BpmnXML { get; set; }
    }
}
