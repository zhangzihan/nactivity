namespace Sys.Workflow.engine.repository
{
    using Sys.Workflow.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="Sys.Workflow.engine.repository.IDeployment"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeDeploymentQuery : INativeQuery<INativeDeploymentQuery, IDeployment>
    {

    }
}