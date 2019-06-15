using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.api.runtime.shared.query
{

    /// <summary>
    /// 
    /// </summary>
    public class PageImpl<T> : IPage<T>
    {
        private IEnumerable<T> content;
        private long totalItems;


        /// <summary>
        /// 
        /// </summary>
        public PageImpl(IEnumerable<T> content,
                        long totalItems)
        {
            this.content = content;
            this.totalItems = totalItems;
        }

        /// <summary>
        /// 
        /// </summary>

        public IEnumerable<T> GetContent()
        {
            return content;
        }

        /// <summary>
        /// 
        /// </summary>

        public long GetTotalItems()
        {
            return totalItems;
        }
    }
}
