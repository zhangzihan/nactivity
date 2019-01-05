using MassTransit;
using org.activiti.engine.impl.cfg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Bpm.Engine.impl
{
    public class GuidGenerator : IIdGenerator
    {
        public string NextId
        {
            get
            {
                return NewId.NextGuid().ToString();
            }
        }
    }
}
