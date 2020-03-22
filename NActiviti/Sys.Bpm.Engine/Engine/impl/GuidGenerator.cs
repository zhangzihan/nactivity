using MassTransit;
using Microsoft.Extensions.Logging;
using Sys.Workflow.Engine.Impl.Cfg;
using Sys.Workflow.Engine.Impl.Persistence.Entity;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Workflow.Engine.Impl
{
    public class GuidGenerator : IIdGenerator
    {
        private Queue<string> ids = new Queue<string>();

        public GuidGenerator()
        {
            Task.Run(PrepareNextIds);
        }

        public string GetNextId()
        {
            if (ids.Count < 5)
            {
                Task.Run(PrepareNextIds);
            }

            return ids.Dequeue();
        }

        private void PrepareNextIds()
        {
            for (var idx = 0; idx < 100; idx++)
            {
                ids.Enqueue(NewId.NextGuid().ToString());
            }
        }
    }
}
