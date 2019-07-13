using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Sys.Workflow;
using Sys.Workflow.Engine.Impl.DB;
using Sys.Workflow.Transactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sys
{
    public class ClassLoader
    {
        private static readonly Regex regex = new Regex("^(http|https):", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public IEnumerator<Uri> GetResources(string v)
        {
            return new List<Uri>()
            {
                GetResource(v)
            }.GetEnumerator();
        }

        public Stream GetResourceAsStream(string name)
        {
            var uri = GetResource(name);

            return uri.OpenStream();
        }

        public Uri GetResource(string name)
        {
            if (regex.IsMatch(name))
            {
                return new Uri(name);
            }

            if (Path.IsPathRooted(name) && File.Exists(name))
            {
                return new Uri(name);
            }

            return new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name));
        }
    }

    public class Properties : JObject
    {
        private static ILogger<Properties> log = ProcessEngineServiceProvider.LoggerService<Properties>();

        public string GetProperty(string key)
        {
            this.TryGetValue(key ?? "", out var val);

            return val?.ToString();
        }

        public void Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Load(fs);
            }
        }

        public void Load(FileStream propertiesFile)
        {
            propertiesFile.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(propertiesFile);
            JObject props = Parse(sr.ReadToEnd());
            foreach (var prop in props.Properties())
            {
                this[prop.Path] = prop.Value;
            }
        }
    }

    public class Environment
    {
        private readonly string v;
        private readonly ITransactionFactory transactionFactory;

        public Environment(string v, ITransactionFactory transactionFactory)
        {
            this.v = v;
            this.transactionFactory = transactionFactory;
        }
    }

    public class TypeHandlerRegistry
    {
        private static readonly ILogger<TypeHandlerRegistry> log = ProcessEngineServiceProvider.LoggerService<TypeHandlerRegistry>();

        internal void Register(Type type, object vARCHAR, IbatisVariableTypeHandler ibatisVariableTypeHandler)
        {
            log.LogError("mock class TypeHandlerRegistry.register");
        }
    }

    public class Configuration
    {
        private static readonly ILogger<Configuration> log = ProcessEngineServiceProvider.LoggerService<Configuration>();

        public string DatabaseId { get; internal set; }

        public Environment Environment { get; internal set; }

        public TypeHandlerRegistry TypeHandlerRegistry { get; internal set; }

        public void AddMapper(Type clazz)
        {
            log.LogError("mock class Configuration.register");
        }

        public Dictionary<string, XNode> SqlFragments
        { get; set; }
    }

    public class ServiceLoader
    {
        private static ILogger<ServiceLoader> log = ProcessEngineServiceProvider.LoggerService<ServiceLoader>();

        public static T Load<T>(ClassLoader classLoader)
        {
            log.LogError("mock class ServiceLoader.load");

            return default;
        }
    }

    public class XMLMapperBuilder
    {
        private static ILogger<XMLMapperBuilder> log = ProcessEngineServiceProvider.LoggerService<XMLMapperBuilder>();

        private readonly Stream stream;
        private readonly Configuration configuration;
        private readonly string resource;
        private readonly Dictionary<string, XNode> sqlFragments;

        public XMLMapperBuilder(Stream stream, Configuration configuration, string resource, Dictionary<string, XNode> sqlFragments)
        {
            this.stream = stream;
            this.configuration = configuration;
            this.resource = resource;
            this.sqlFragments = sqlFragments;
        }

        public void Parse()
        {
            log.LogError("mock class XMLMapperBuilder.parse");
        }
    }

    public class XMLConfigBuilder
    {
        private static ILogger<XMLConfigBuilder> log = ProcessEngineServiceProvider.LoggerService<XMLConfigBuilder>();

        private readonly StreamReader reader;
        private readonly string v;
        private readonly Properties properties;

        public XMLConfigBuilder(StreamReader reader, string v, Properties properties)
        {
            this.reader = reader;
            this.v = v;
            this.properties = properties;
        }

        public Configuration Configuration { get; internal set; }

        public Configuration Parse()
        {
            log.LogError("mock class XMLConfigBuilder.parse");

            return new Configuration();
        }
    }
}