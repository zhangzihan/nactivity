using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Spring.Expressions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using Sys.Workflow.Util;

namespace Sys.Workflow
{
    /// <summary>
    /// 
    /// </summary>
    public class ExternalConnectorProvider
    {
        /// <summary>
        /// 
        /// </summary>
        public IConfiguration Configuration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public object ExternalConnectorContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ExternalConnectorProvider()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public ExternalConnectorProvider(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));

            }
            this.Configuration = configuration;
        }

        /// <summary>
        /// 获取部门下所有人员 External Web API,
        /// </summary>
        public virtual string GetUserByDept
        {
            get => ResolveServiceUrl("GetUserByDept");
        }

        /// <summary>
        /// 获取人员信息External Web API
        /// </summary>
        public virtual string GetUserByUser
        {
            get => ResolveServiceUrl("GetUserByUser");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string GetUser
        {
            get => ResolveServiceUrl("GetUser");
        }

        /// <summary>
        /// 获取部门领导 External Web API
        /// </summary>
        public virtual string GetUserByDeptLeader
        {
            get => ResolveServiceUrl("GetUserByDeptLeader");
        }

        /// <summary>
        /// 获取直接汇报人 External Web API
        /// </summary>
        public virtual string GetUserByDirectReporter
        {
            get => ResolveServiceUrl("GetUserByDirectReporter");
        }

        /// <summary>
        /// 获取担当某个岗位角色的人员 External Web API
        /// </summary>
        public virtual string GetUserByDuty
        {
            get => ResolveServiceUrl("GetUserByDuty");
        }

        /// <summary>
        /// 获取直接下属 External Web API
        /// </summary>
        public virtual string GetUserByUnderling
        {
            get => ResolveServiceUrl("GetUserByUnderling");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string MailServiceUrl
        {
            get => ResolveServiceUrl("MailServiceUrl");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string WechatServiceUrl
        {
            get => ResolveServiceUrl("WechatServiceUrl");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string SmsServiceUrl
        {
            get => ResolveServiceUrl("SmsServiceUrl");
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual string WorkflowUrl
        {
            get => ResolveServiceUrl("WorkflowUrl");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual string ResolveServiceUrl(string key)
        {
            ConfigUtil.Configuration = Configuration;

            string url = Configuration.GetSection($"ExternalConnectorUrl:{key}")?.Value;

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new NotExistsExternalConnectorUrlException(key);
            }

            return ResolveUrl(url);
        }

        public virtual string ResolveUrl(string url)
        {
            if (ExternalConnectorContext == null)
            {
                IEnumerable<IConfigurationSection> childs = Configuration.GetSection("ExternalConnectorUrl").GetChildren();

                ExternalConnectorContext = childs.ToDictionary(x => x.Key, x => x.Value);
            }

            IExpression expr = Expression.Parse(url);

            return expr.GetValue(ExternalConnectorContext).ToString();
        }

        public virtual string DiscoverServiceUrl(string serviceName)
        {
            return ResolveServiceUrl(serviceName);
        }
    }

    [Serializable]
    public class NotExistsExternalConnectorUrlException : Exception
    {
        public NotExistsExternalConnectorUrlException(string key) :
            base($"不存在的外部配置URL,ExternalConnectorUrl:{key}")
        {
        }

        protected NotExistsExternalConnectorUrlException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}
