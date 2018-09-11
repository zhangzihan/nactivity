namespace org.activiti.engine.runtime
{
	using org.activiti.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="IProcessInstance"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessInstanceQuery : INativeQuery<INativeProcessInstanceQuery, IProcessInstance>
	{

	}

}