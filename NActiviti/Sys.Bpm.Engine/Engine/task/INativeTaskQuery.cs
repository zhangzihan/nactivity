namespace Sys.Workflow.engine.task
{
	using Sys.Workflow.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="ITask"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeTaskQuery : INativeQuery<INativeTaskQuery, ITask>
	{

	}

}