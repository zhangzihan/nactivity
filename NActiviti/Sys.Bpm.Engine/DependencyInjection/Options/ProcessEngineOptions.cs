using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Options
{
    /// <summary>
    /// 流程引擎配置信息
    /// </summary>
    public class ProcessEngineOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public ProcessEngineOptions()
        {
        }

        /// <summary>
        /// 数据源
        /// </summary>
        public DataSourceOption DataSource { get; set; } = new DataSourceOption();

        /// <summary>
        /// 外部Web服务Url提供类
        /// </summary>
        public ExternalConnectorProvider ExternalConnector { get; set; } = new ExternalConnectorProvider();
    }
}
