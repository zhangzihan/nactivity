using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SmartSql.Configuration;
using SmartSql.Configuration.Statements;
using SmartSql.Configuration.Maps;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SmartSql.Abstractions.Config
{
    public abstract class ConfigLoader : IConfigLoader
    {
        private StatementFactory _statementFactory = new StatementFactory();

        public SmartSqlMapConfig SqlMapConfig { get; set; }

        public abstract event OnChangedHandler OnChanged;

        public void Dispose()
        {
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);

        public abstract SmartSqlMapConfig Load();

        public SmartSqlMapConfig LoadConfig(ConfigStream configStream)
        {
            using (configStream.Stream)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(SmartSqlMapConfig));
                SqlMapConfig = xmlSerializer.Deserialize(configStream.Stream) as SmartSqlMapConfig;
                SqlMapConfig.Path = configStream.Path;
                SqlMapConfig.SmartSqlMaps = new List<SmartSqlMap> { };
                if (SqlMapConfig.TypeHandlers is object)
                {
                    foreach (var typeHandler in SqlMapConfig.TypeHandlers)
                    {
                        typeHandler.Handler = TypeHandlerFactory.Create(typeHandler.Type);
                    }
                }
                return SqlMapConfig;
            }
        }
        public SmartSqlMap LoadSmartSqlMap(ConfigStream configStream)
        {
            try
            {
                using (configStream.Stream)
                {
                    var sqlMap = new SmartSqlMap
                    {
                        SqlMapConfig = SqlMapConfig,
                        Path = configStream.Path,
                        Statements = new List<Statement> { },
                        Caches = new List<Configuration.Cache> { },
                        ResultMaps = new List<ResultMap> { },
                        ParameterMaps = new List<ParameterMap> { }
                    };

                    XDocument xmlDoc = XDocument.Load(configStream.Stream, LoadOptions.SetLineInfo);
                    var nametable = xmlDoc.Root.CreateReader().NameTable;
                    XmlNamespaceManager xmlNsM = new XmlNamespaceManager(nametable);
                    xmlNsM.AddNamespace("ns", "http://SmartSql.net/schemas/SmartSqlMap.xsd");
                    sqlMap.Scope = xmlDoc.Root.XPathSelectElement("//ns:SmartSqlMap", xmlNsM)
                        .Attribute("Scope").Value;

                    #region Init Caches
                    var cacheNodes = xmlDoc.XPathSelectElements("//ns:Cache", xmlNsM);
                    foreach (XElement cacheNode in cacheNodes)
                    {
                        var cache = CacheFactory.Load(cacheNode);
                        sqlMap.Caches.Add(cache);
                    }
                    #endregion

                    #region Init ResultMaps
                    var resultMapsNodes = xmlDoc.XPathSelectElements("//ns:ResultMap", xmlNsM);
                    foreach (XElement xmlNode in resultMapsNodes)
                    {
                        var resultMap = MapFactory.LoadResultMap(xmlNode, SqlMapConfig);
                        sqlMap.ResultMaps.Add(resultMap);
                    }
                    #endregion
                    #region Init ParameterMaps
                    var parameterMaps = xmlDoc.XPathSelectElements("//ns:ParameterMap", xmlNsM);
                    foreach (XElement xmlNode in parameterMaps)
                    {
                        var parameterMap = MapFactory.LoadParameterMap(xmlNode, SqlMapConfig);
                        sqlMap.ParameterMaps.Add(parameterMap);
                    }
                    #endregion

                    #region Init Statement
                    var statementNodes = xmlDoc.XPathSelectElements("//ns:Statement", xmlNsM);
                    LoadStatementInSqlMap(sqlMap, statementNodes);

                    var insertNodes = xmlDoc.XPathSelectElements("//ns:Insert", xmlNsM);
                    LoadStatementInSqlMap(sqlMap, insertNodes);

                    var updateNodes = xmlDoc.XPathSelectElements("//ns:Update", xmlNsM);
                    LoadStatementInSqlMap(sqlMap, updateNodes);

                    var deleteNodes = xmlDoc.XPathSelectElements("//ns:Delete", xmlNsM);
                    LoadStatementInSqlMap(sqlMap, deleteNodes);

                    var selectNodes = xmlDoc.XPathSelectElements("//ns:Select", xmlNsM);
                    LoadStatementInSqlMap(sqlMap, selectNodes);
                    #endregion

                    return sqlMap;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(configStream.Path, ex);
            }
        }

        private void LoadStatementInSqlMap(SmartSqlMap sqlMap, IEnumerable<XElement> statementNodes)
        {
            foreach (XElement statementNode in statementNodes)
            {
                var statement = _statementFactory.Load(statementNode, sqlMap);
                sqlMap.Statements.Add(statement);
            }
        }
    }

    public class ConfigStream
    {
        public String Path { get; set; }
        public Stream Stream { get; set; }
    }
}
