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
        IEnumerable<T> GetContent();


        /// <summary>
        /// 
        /// </summary>
        long GetTotalItems();
    }
}
