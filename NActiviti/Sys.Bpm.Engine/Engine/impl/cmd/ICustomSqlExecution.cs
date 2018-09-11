using System;

namespace org.activiti.engine.impl.cmd
{
	/// 
	public interface ICustomSqlExecution<Mapper, ResultType>
	{

	  Type MapperClass {get;}

	  ResultType execute(Mapper mapper);

	}
}