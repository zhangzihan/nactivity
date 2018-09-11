namespace org.activiti.engine.repository
{
	using org.activiti.engine.query;

	/// <summary>
	/// Allows querying of <seealso cref="org.activiti.engine.repository.IProcessDefinition"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessDefinitionQuery : INativeQuery<INativeProcessDefinitionQuery, IProcessDefinition>
	{

	}
}