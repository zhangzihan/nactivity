namespace Sys.Workflow.Engine.Tasks
{
	using Sys.Workflow.Engine.Query;

	/// <summary>
	/// Allows querying of <seealso cref="ITask"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeTaskQuery : INativeQuery<INativeTaskQuery, ITask>
	{

	}

}