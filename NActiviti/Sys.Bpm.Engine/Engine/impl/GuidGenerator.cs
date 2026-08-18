using MassTransit;
using Sys.Workflow.Engine.Impl.Cfg;

namespace Sys.Workflow.Engine.Impl
{
    public class GuidGenerator : IIdGenerator
    {
        public string GetNextId()
        {
            // MassTransit 的 NewId 本身是高效的线程安全 ID 生成器，直接生成即可（与 StrongUuidGenerator 格式一致）。
            // 原实现的静态预生成队列在队空时会 Task.Delay 阻塞并递归调用，存在空转风险，已移除。
            return NewId.NextGuid().ToString();
        }
    }
}
