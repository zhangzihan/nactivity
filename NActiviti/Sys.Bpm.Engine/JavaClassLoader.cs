using javax.transaction;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using org.activiti.engine.impl.db;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Sys
{
    public class ClassLoader
    {
        private Regex regex = new Regex("^(http|https):", RegexOptions.IgnoreCase);

        public IEnumerator<Uri> getResources(string v)
        {
            return new List<Uri>()
            {
                getResource(v)
            }.GetEnumerator();
        }

        public Stream getResourceAsStream(string name)
        {
            var uri = getResource(name);

            return uri.openStream();
        }

        public Uri getResource(string name)
        {
            if (regex.IsMatch(name))
            {
                return new Uri(name);
            }

            if (Path.IsPathRooted(name) && File.Exists(name))
            {
                return new Uri(name);
            }

            var dir = Path.GetDirectoryName(new Uri(this.GetType().Assembly.CodeBase).LocalPath);
            return new Uri(Path.Combine(dir, name));
        }
    }

    public class Properties : JObject
    {
        private static ILogger<Properties> log = ProcessEngineServiceProvider.LoggerService<Properties>();

        public string getProperty(string key)
        {
            this.TryGetValue(key ?? "", out var val);

            return val?.ToString();
        }

        public void load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            using (FileStream fs = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                load(fs);
            }
        }

        public void load(FileStream propertiesFile)
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
        private string v;
        private ITransactionFactory transactionFactory;

        public Environment(string v, ITransactionFactory transactionFactory)
        {
            this.v = v;
            this.transactionFactory = transactionFactory;
        }
    }

    public class TypeHandlerRegistry
    {
        private static ILogger<TypeHandlerRegistry> log = ProcessEngineServiceProvider.LoggerService<TypeHandlerRegistry>();

        internal void register(Type type, object vARCHAR, IbatisVariableTypeHandler ibatisVariableTypeHandler)
        {
            log.LogError("mock class TypeHandlerRegistry.register");
        }
    }

    public class Configuration
    {
        private static ILogger<Configuration> log = ProcessEngineServiceProvider.LoggerService<Configuration>();

        public string DatabaseId { get; internal set; }

        public Environment Environment { get; internal set; }

        public TypeHandlerRegistry TypeHandlerRegistry { get; internal set; }

        public void addMapper(Type clazz)
        {
            log.LogError("mock class Configuration.register");
        }

        public Dictionary<string, XNode> SqlFragments
        { get; set; }
    }

    public class ServiceLoader
    {
        private static ILogger<ServiceLoader> log = ProcessEngineServiceProvider.LoggerService<ServiceLoader>();

        public static T load<T>(ClassLoader classLoader)
        {
            log.LogError("mock class ServiceLoader.load");

            return default(T);
        }
    }

    public class XMLMapperBuilder
    {
        private static ILogger<XMLMapperBuilder> log = ProcessEngineServiceProvider.LoggerService<XMLMapperBuilder>();

        private Stream stream;
        private Configuration configuration;
        private string resource;
        private Dictionary<string, XNode> sqlFragments;

        public XMLMapperBuilder(Stream stream, Configuration configuration, string resource, Dictionary<string, XNode> sqlFragments)
        {
            this.stream = stream;
            this.configuration = configuration;
            this.resource = resource;
            this.sqlFragments = sqlFragments;
        }

        public void parse()
        {
            log.LogError("mock class XMLMapperBuilder.parse");
        }
    }

    public class XMLConfigBuilder
    {
        private static ILogger<XMLConfigBuilder> log = ProcessEngineServiceProvider.LoggerService<XMLConfigBuilder>();

        private StreamReader reader;
        private string v;
        private Properties properties;

        public XMLConfigBuilder(StreamReader reader, string v, Properties properties)
        {
            this.reader = reader;
            this.v = v;
            this.properties = properties;
        }

        public Configuration Configuration { get; internal set; }

        public Configuration parse()
        {
            log.LogError("mock class XMLConfigBuilder.parse");

            return new Configuration();
        }
    }
}