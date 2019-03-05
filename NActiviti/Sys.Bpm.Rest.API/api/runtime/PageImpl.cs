using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.api.runtime.shared.query
{
    public class PageImpl<T> : IPage<T>
    {
        private IList<T> content;
        private long totalItems;

        public PageImpl(IList<T> content,
                        long totalItems)
        {
            this.content = content;
            this.totalItems = totalItems;
        }

        public IList<T> getContent()
        {
            return content;
        }

        public long getTotalItems()
        {
            return totalItems;
        }
    }
}
