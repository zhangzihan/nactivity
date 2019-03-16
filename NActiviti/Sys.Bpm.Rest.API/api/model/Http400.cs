using System.Collections.Generic;

namespace Sys.Bpm.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public class Http400
    {
        /// <summary>
        /// 
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private IList<Http400> details;
        public IList<Http400> Details
        {
            get
            {
                if (details == null)
                {
                    details = new List<Http400>();
                }
                return details;
            }
            set => details = value;
        }
    }
}
