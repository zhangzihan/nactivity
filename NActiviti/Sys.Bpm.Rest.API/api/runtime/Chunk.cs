
using System.Collections;
using System.Collections.Generic;

namespace org.activiti.api.runtime.shared.query
{
    public abstract class Chunk<T> : ISlice<T>
    {
        protected IList<T> list;
        protected Pageable pageable;

        public Chunk(IList<T> list, Pageable pageable)
        {
            this.list = list;
            this.pageable = pageable;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
    }
}
