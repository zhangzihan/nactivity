using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.core
{
    /// <summary>
    /// Token验证配置
    /// </summary>
    public class SecurityPoliciesProviderOptions
    {
        /// <summary>
        /// 
        /// </summary>
        public SecurityPoliciesProviderOptions()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        public SecurityPoliciesProviderOptions(IConfiguration configuration)
        {
            configuration.Bind(this);
        }

        /// <summary>
        /// 鉴权服务器路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 鉴权发行人
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(2);

        /// <summary>
        /// 签名证书
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }
    }
}
