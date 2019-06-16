using System;

namespace Sys.Workflow.Cloud.Services.Core
{
    /// <summary>
    /// 
    /// </summary>
	public class ActivitiForbiddenException : Exception
	{
		private const long serialVersionUID = 1L;

        /// <summary>
        /// 
        /// </summary>
        public ActivitiForbiddenException(string message, Exception cause) : base(message, cause)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        public ActivitiForbiddenException(string message) : base(message)
		{
		}
	}
}