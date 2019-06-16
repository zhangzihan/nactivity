namespace Sys.Workflow.Engine.Runtime
{
	using Sys.Workflow.Engine.Query;

	/// <summary>
	/// Allows querying of <seealso cref="IExecution"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeExecutionQuery : INativeQuery<INativeExecutionQuery, IExecution>
	{

	}

}