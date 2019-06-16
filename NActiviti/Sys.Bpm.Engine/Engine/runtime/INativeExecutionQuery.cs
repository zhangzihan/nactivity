namespace Sys.Workflow.engine.runtime
{
	using Sys.Workflow.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="IExecution"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeExecutionQuery : INativeQuery<INativeExecutionQuery, IExecution>
	{

	}

}