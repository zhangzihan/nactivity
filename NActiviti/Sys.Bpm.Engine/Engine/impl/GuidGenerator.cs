using MassTransit;
using Sys.Workflow.Engine.Impl.Cfg;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl
{
    public class GuidGenerator : IIdGenerator
    {
        private static readonly ConcurrentQueue<string> ids = new ConcurrentQueue<string>();

        public GuidGenerator()
        {
            PrepareNextIds();
        }

        public string GetNextId()
        {
            if (ids.TryDequeue(out var id) && !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }
            if (ids.Count < 10)
            {
                Task.Run(PrepareNextIds);
                Task.Delay(10).GetAwaiter().GetResult();
            }
            return GetNextId();
        }

        private void PrepareNextIds()
        {
            for (var idx = 0; idx < 100; idx++)
            {
                var id = NewId.NextGuid().ToString();
                if (id is null)
                {
                    idx -= 1;
                    continue;
                }
                ids.Enqueue(id);
            }
        }
    }
}
