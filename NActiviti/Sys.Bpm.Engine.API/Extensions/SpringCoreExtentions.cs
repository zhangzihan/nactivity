using Microsoft.Extensions.DependencyInjection;
using Sys.Workflow.Engine.Impl.Util;
using Spring.Core.TypeResolution;
using Sys.Workflow.Engine.Api;
using Sys.Workflow.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow
{
    public static class SpringCoreExtentions
    {
        public static void AddSpringCoreTypeRepository(this IServiceCollection services)
        {
            TypeRegistry.RegisterType(typeof(CollectionUtil));
            TypeRegistry.RegisterType(typeof(ConfigUtil));
            TypeRegistry.RegisterType(typeof(DateTimeHelper));
            TypeRegistry.RegisterType(typeof(UrlUtil));
            TypeRegistry.RegisterType(typeof(Math));
            TypeRegistry.RegisterType(typeof(MathHelper));
        }
    }
}
