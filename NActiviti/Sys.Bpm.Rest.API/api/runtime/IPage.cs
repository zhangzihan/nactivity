using System.Collections.Generic;

namespace Sys.Workflow.Api.Runtime.Shared.Query
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
