using System.Collections.Generic;

namespace org.activiti.api.runtime.shared.query
{
    public interface IPage<T>
    {
        IList<T> getContent();

        long getTotalItems();
    }
}
