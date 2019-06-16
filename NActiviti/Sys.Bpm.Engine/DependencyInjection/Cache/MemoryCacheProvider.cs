using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Caches
{
    public class MemoryCacheProvider
    {
        public IMemoryCache Create(int sizeLimit)
        {
            var opts = new MemoryCacheOptions();
            if (sizeLimit > 0)
            {
                opts.SizeLimit = sizeLimit;
            }
            return Create(opts);
        }

        public IMemoryCache Create(MemoryCacheOptions options)
        {
            return new MemoryCache(options);
        }
    }
}
