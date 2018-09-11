using System.Collections.Generic;

namespace org.activiti.engine.impl.@event.logger
{

	using org.activiti.engine.impl.@event.logger.handler;
	using org.activiti.engine.impl.interceptor;

	/// 
	public interface IEventFlusher : ICommandContextCloseListener
	{

	  IList<IEventLoggerEventHandler> EventHandlers {get;set;}


	  void addEventHandler(IEventLoggerEventHandler databaseEventLoggerEventHandler);

	}

}