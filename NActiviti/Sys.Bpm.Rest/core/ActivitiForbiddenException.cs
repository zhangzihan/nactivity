using System;

namespace org.activiti.cloud.services.core
{
	public class ActivitiForbiddenException : Exception
	{
		private const long serialVersionUID = 1L;

		public ActivitiForbiddenException(string message, Exception cause) : base(message, cause)
		{
		}

		public ActivitiForbiddenException(string message) : base(message)
		{
		}
	}
}