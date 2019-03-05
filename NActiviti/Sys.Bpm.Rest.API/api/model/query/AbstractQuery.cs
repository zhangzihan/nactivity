using org.activiti.api.runtime.shared.query;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.cloud.services.api.model
{
    /// <summary>
    /// 查询对象
    /// </summary>
    public abstract class AbstractQuery
    {
        private Pageable _pageable;

        /// <summary>
        /// 分页
        /// </summary>
        public virtual Pageable Pageable
        {
            get
            {
                if (_pageable == null)
                {
                    _pageable = new Pageable();
                }
                return _pageable;
            }
            set => _pageable = value ?? new Pageable();
        }

        /// <summary>
        /// 实体id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// 实例对象名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 包含实例对象名称
        /// </summary>
        public string NameLike { get; set; }
    }
}
