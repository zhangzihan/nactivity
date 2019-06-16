using System;

namespace Sys.Workflow.engine.impl.cmd
{
	/// 
	public interface ICustomSqlExecution<Mapper, ResultType>
	{

	  Type MapperClass {get;}

	  ResultType execute(Mapper mapper);

	}
}