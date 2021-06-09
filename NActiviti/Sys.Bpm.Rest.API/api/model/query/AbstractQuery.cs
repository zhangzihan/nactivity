using Sys.Workflow.Api.Runtime.Shared.Query;

namespace Sys.Workflow.Cloud.Services.Api.Model
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
                if (_pageable is null)
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
        public virtual string Id { get; set; }

        /// <summary>
        /// 租户id
        /// </summary>
        public virtual string TenantId { get; set; } = "";

        /// <summary>
        /// 实例对象名称
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// 包含实例对象名称
        /// </summary>
        public virtual string NameLike { get; set; }
    }
}
