namespace Sys.Workflow.Cloud.Services.Api.Commands
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