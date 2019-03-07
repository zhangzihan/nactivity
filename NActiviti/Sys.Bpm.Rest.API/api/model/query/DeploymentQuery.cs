﻿using org.activiti.api.runtime.shared.query;
using org.activiti.engine.impl.persistence.entity;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class DeploymentQuery : AbstractQuery
    {
        public DeploymentQuery()
        {

        }

        /// <summary>
        /// 多个部署id
        /// </summary>
        public string[] Ids { get; set; }

        /// <summary>
        /// 部署目录
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 包含部署目录
        /// </summary>
        public string CategoryLike { get; set; }

        /// <summary>
        /// 不等于某个目录
        /// </summary>
        public string CategoryNotEquals { get; set; }

        /// <summary>
        /// 关联业务数据键值
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 关联业务数据键值
        /// </summary>
        public string KeyLike { get; set; }

        /// <summary>
        /// 多个关联业务数据键值
        /// </summary>
        public string[] Keys { get; set; }

        /// <summary>
        /// 关联业务键值
        /// </summary>
        public string BusinessKey { get; set; }

        /// <summary>
        /// 不使用租户id
        /// </summary>
        public bool WithoutTenantId { get; set; }

        /// <summary>
        /// 仅查询最后部署的流程
        /// </summary>
        public bool LatestDeployment { get; set; }
    }
}
