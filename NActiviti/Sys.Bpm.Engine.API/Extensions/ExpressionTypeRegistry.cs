using Spring.Core.TypeResolution;
using Sys.Workflow.Engine.Api;
using Sys.Workflow.Engine.Impl.Util;
using Sys.Workflow.Util;
using System;
using System.IO;

namespace Sys.Expressions
{
    public class ExpressionTypeRegistry
    {
        static ExpressionTypeRegistry()
        {
            TypeRegistry.RegisterType(typeof(CollectionUtil));
            TypeRegistry.RegisterType(typeof(ConfigUtil));
            TypeRegistry.RegisterType(typeof(DateTimeHelper));
            TypeRegistry.RegisterType(typeof(UrlUtil));
            TypeRegistry.RegisterType(typeof(Math));
            TypeRegistry.RegisterType(typeof(String));
            TypeRegistry.RegisterType(typeof(MathHelper));
        }

        private static void LoadFromJson()
        {
            string jsonFile = Path.Combine(Directory.GetCurrentDirectory(), @"config\formulas.json");

            if (File.Exists(jsonFile) == false)
            {

            }
        }

        public ExpressionTypeRegistry Register(Type type)
        {
            TypeRegistry.RegisterType(type);

            return this;
        }

        public ExpressionTypeRegistry Register(string alias, string type)
        {
            TypeRegistry.RegisterType(alias, type);

            return this;
        }
    }
}
