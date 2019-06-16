namespace Sys.Workflow.Engine.Repository
{
	using Sys.Workflow.Engine.Query;

	/// <summary>
	/// Allows querying of <seealso cref="Sys.Workflow.Engine.Repository.IModel"/>s via native (SQL) queries
	/// 
	/// 
	/// </summary>
	public interface INativeModelQuery : INativeQuery<INativeModelQuery, IModel>
	{

	}
}