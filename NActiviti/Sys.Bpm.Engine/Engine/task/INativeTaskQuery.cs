namespace org.activiti.engine.task
{
	using org.activiti.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="ITask"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeTaskQuery : INativeQuery<INativeTaskQuery, ITask>
	{

	}

}