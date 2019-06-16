using System.Collections.Generic;

namespace Sys.Workflow.api.runtime.shared.query
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
