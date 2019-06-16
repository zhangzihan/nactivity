namespace Sys.Workflow.Engine.Repository
{
    using Sys.Workflow.Engine.Query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.Engine.Repository.IDeployment"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeDeploymentQuery : INativeQuery<INativeDeploymentQuery, IDeployment>
    {

    }
}