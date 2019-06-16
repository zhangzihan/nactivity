using System;

namespace Sys.Workflow.Engine.Impl.Cmd
{
	/// 
	public interface ICustomSqlExecution<Mapper, ResultType>
	{

	  Type MapperClass {get;}

	  ResultType execute(Mapper mapper);

	}
}