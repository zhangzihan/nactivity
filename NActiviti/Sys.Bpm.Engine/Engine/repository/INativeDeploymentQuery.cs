namespace org.activiti.engine.repository
{
    using org.activiti.engine.query;

    /// <summary>
    /// Allows querying of <seealso cref="org.activiti.engine.repository.IDeployment"/>s via native (SQL) queries
    /// 
    /// 
    /// </summary>
    public interface INativeDeploymentQuery : INativeQuery<INativeDeploymentQuery, IDeployment>
    {

    }
}