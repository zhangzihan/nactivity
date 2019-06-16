namespace Sys.Workflow.Engine.Runtime
{
	using Sys.Workflow.Engine.Query;

	/// <summary>
	/// Allows querying of <seealso cref="IProcessInstance"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessInstanceQuery : INativeQuery<INativeProcessInstanceQuery, IProcessInstance>
	{

	}

}