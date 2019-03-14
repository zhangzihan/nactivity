using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{

    /// <summary>
    /// 流程部署DTO
    /// </summary>
    public class Deployment
    {
        private string id;
        private string name;
        private string category;
        private string key;
        private string tenantId;
        private string businessKey;
        private DateTime deployTime;


        /// <summary>
        /// 
        /// </summary>
        public Deployment()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">部署id</param>
        /// <param name="name">流程名称</param>
        /// <param name="category">流程目录</param>
        /// <param name="tenantId">租户id</param>
        /// <param name="businessKey">业务键值</param>
        /// <param name="deployTime">部署时间</param>

        public Deployment(string id, string name, string category, string tenantId, string businessKey, DateTime deployTime)
        {
            this.id = id;
            this.name = name;
            this.category = category;
            this.tenantId = tenantId;
            this.businessKey = businessKey;
            this.deployTime = deployTime;
        }

        /// <summary>
        /// 部署id
        /// </summary>

        public string Id { get => id; set => id = value; }


        /// <summary>
        /// 流程名称
        /// </summary>
        public string Name { get => name; set => name = value; }


        /// <summary>
        /// 流程目录
        /// </summary>
        public string Category { get => category; set => category = value; }


        /// <summary>
        /// 流程键值
        /// </summary>
        public string Key { get => key; set => key = value; }


        /// <summary>
        /// 租户id
        /// </summary>
        public string TenantId { get => tenantId; set => tenantId = value; }


        /// <summary>
        /// 业务键值
        /// </summary>
        public string BusinessKey { get => businessKey; set => businessKey = value; }


        /// <summary>
        /// 部署时间
        /// </summary>
        public DateTime DeployTime { get => deployTime; set => deployTime = value; }
    }
}
