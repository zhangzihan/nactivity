using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Core.Cache
{
    public class InMemoryCacheOptions
    {
        public int? SizeLimit { get; set; }

        public TimeSpan? AbsoluteExpiration { get; set; }
    }
}
