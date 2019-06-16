namespace Sys.Workflow.engine.runtime
{
	using Sys.Workflow.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="IProcessInstance"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessInstanceQuery : INativeQuery<INativeProcessInstanceQuery, IProcessInstance>
	{

	}

}