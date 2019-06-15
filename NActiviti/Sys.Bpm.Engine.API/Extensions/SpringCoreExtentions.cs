using Microsoft.Extensions.DependencyInjection;
using org.activiti.engine.impl.util;
using Spring.Core.TypeResolution;
using Sys.Bpm.Engine.API.Engine;
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
