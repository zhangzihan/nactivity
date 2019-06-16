
using Newtonsoft.Json;

namespace Sys.Workflow.Api.Runtime.Shared.Query
{
    /// <summary>
    /// 分页
    /// </summary>
    public class Pageable
    {

        /// <summary>
        /// 
        /// </summary>
        public Pageable()
        {

        }

        /// <summary>
        /// 排序
        /// </summary>
        public Sort Sort { get; set; }

        /// <summary>
        /// 页编号
        /// </summary>
        public int PageNo { get; set; } = 1;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; set; } = 10;
    }
}
