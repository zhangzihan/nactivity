namespace Sys.Workflow.Engine.Repository
{
	using Sys.Workflow.Engine.Query;

	/// <summary>
	/// Allows querying of <seealso cref="Sys.Workflow.Engine.Repository.IProcessDefinition"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeProcessDefinitionQuery : INativeQuery<INativeProcessDefinitionQuery, IProcessDefinition>
	{

	}
}