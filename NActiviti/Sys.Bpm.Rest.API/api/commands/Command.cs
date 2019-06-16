namespace Sys.Workflow.cloud.services.api.commands
{
    /// <summary>
    /// 命令处理接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 命令id
        /// </summary>
        string Id { get; }
    }

}