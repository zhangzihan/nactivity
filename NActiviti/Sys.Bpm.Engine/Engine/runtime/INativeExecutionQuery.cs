namespace org.activiti.engine.runtime
{
	using org.activiti.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="IExecution"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeExecutionQuery : INativeQuery<INativeExecutionQuery, IExecution>
	{

	}

}