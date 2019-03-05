using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{
    public class Deployment
    {
        private string id;
        private string name;
        private string category;
        private string key;
        private string tenantId;
        private string businessKey;
        private DateTime deployTime;

        public Deployment()
        {

        }

        public Deployment(string id, string name, string tenantId, string businessKey, DateTime deployTime)
        {
            this.id = id;
            this.name = name;
            this.tenantId = tenantId;
            this.businessKey = businessKey;
            this.deployTime = deployTime;
        }

        public string Id { get => id; set => id = value; }

        public string Name { get => name; set => name = value; }

        public string Category { get => category; set => category = value; }

        public string Key { get => key; set => key = value; }

        public string TenantId { get => tenantId; set => tenantId = value; }

        public string BusinessKey { get => businessKey; set => businessKey = value; }

        public DateTime DeployTime { get => deployTime; set => deployTime = value; }
    }
}
