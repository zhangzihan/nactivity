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
        private IList<T> content;
        private long totalItems;


        /// <summary>
        /// 
        /// </summary>
        public PageImpl(IList<T> content,
                        long totalItems)
        {
            this.content = content;
            this.totalItems = totalItems;
        }

        /// <summary>
        /// 
        /// </summary>

        public IList<T> getContent()
        {
            return content;
        }

        /// <summary>
        /// 
        /// </summary>

        public long getTotalItems()
        {
            return totalItems;
        }
    }
}
