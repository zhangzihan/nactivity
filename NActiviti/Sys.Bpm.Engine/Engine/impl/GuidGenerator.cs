using MassTransit;
using Microsoft.Extensions.Logging;
using Sys.Workflow.engine.impl.cfg;
using Sys.Workflow.engine.impl.persistence.entity;
using Sys.Workflow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Engine.impl
{
    public class GuidGenerator : IIdGenerator
    {
        public string GetNextId()
        {
            return NewId.NextGuid().ToString();
        }
    }
}
