using System.Collections.Generic;

namespace org.activiti.api.runtime.shared.query
{

    /// <summary>
    /// 
    /// </summary>
    public interface IPage<T>
    {

        /// <summary>
        /// 
        /// </summary>
        IList<T> getContent();


        /// <summary>
        /// 
        /// </summary>
        long getTotalItems();
    }
}
