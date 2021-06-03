using MassTransit;
using Sys.Workflow.Engine.Impl.Cfg;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl
{
    public class GuidGenerator : IIdGenerator
    {
        private static readonly Queue<string> ids = new Queue<string>();

        public GuidGenerator()
        {
            PrepareNextIds();
        }

        public string GetNextId()
        {
            var id = ids.Dequeue();
            if (ids.Count < 10)
            {
                Task.Run(PrepareNextIds);
            }
            return id;
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
