using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace org.springframework.hateoas
{
    public class Resources<T> : ResourceSupport
    {
        private readonly IEnumerable<T> resourcesList;

        private readonly long total;

        public Resources(IEnumerable<T> resourcesList)
        {
            this.resourcesList = resourcesList;
        }

        [JsonConstructor]
        public Resources([JsonProperty("list")]IEnumerable<T> resourcesList, 
            [JsonProperty("totalCount")]long total, 
            [JsonProperty("pageNo")]int pageNo, 
            [JsonProperty("pageSize")]int pageSize)
        {
            this.resourcesList = resourcesList;
            this.total = total;
            this.PageNo = pageNo;
            this.PageSize = pageSize;
        }

        /// <summary>
        /// 当前页记录数
        /// </summary>
        public long RecordCount { get => resourcesList.Count(); }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageNo { get; } = 0;

        /// <summary>
        /// 每页大小
        /// </summary>
        public int PageSize { get; } = 0;

        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalCount { get => total; }

        public IEnumerable<T> List { get => resourcesList; }
    }
}
