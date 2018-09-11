using SmartSql.Cache;
using SmartSql.Exceptions;
using SmartSql.Configuration.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using SmartSql.Configuration.Statements;
using SmartSql.Configuration.Maps;
using System.Xml.Linq;
using Sys;

namespace SmartSql.Configuration
{
    public class StatementFactory
    {
        private Statement Create(XElement statementNode)
        {
            Enum.TryParse<StatementType>(statementNode.Name.LocalName, out var statementType);
            Statement statement = null;
            switch (statementType)
            {
                case StatementType.Insert:
                    statement = new InsertStatement();
                    break;
                case StatementType.Select:
                    statement = new SelectStatement();
                    break;
                case StatementType.Update:
                    statement = new UpdateStatement();
                    break;
                case StatementType.Delete:
                    statement = new DeleteStatement();
                    break;
                case StatementType.Statement:
                default:
                    statement = new Statement();
                    break;
            }

            statement.Id = statementNode.Attribute("Id").Value;
            statement.SqlTags = new List<ITag>();

            return statement;
        }


        public Statement Load(XElement statementNode, SmartSqlMap smartSqlMap)
        {
            var statement = Create(statementNode);
            statement.SmartSqlMap = smartSqlMap;

            var type = statementNode.GetAttribute("ResultType");
            if (string.IsNullOrWhiteSpace(type) == false)
            {
                statement.ResultType = Type.GetType(type);
            }
            string cacheId = statementNode.GetAttribute("Cache");
            #region Attribute For Cache & ResultMap & ParameterMap
            if (!String.IsNullOrEmpty(cacheId))
            {
                var cache = smartSqlMap.Caches.FirstOrDefault(m => m.Id == cacheId);
                statement.Cache = cache ?? throw new SmartSqlException($"Statement.Id:{statement.Id} can not find Cache.Id:{cacheId}");
            }

            string resultMapId = statementNode.GetAttribute("ResultMap");
            if (!String.IsNullOrEmpty(resultMapId))
            {
                var resultMap = smartSqlMap.ResultMaps.FirstOrDefault(r => r.Id == resultMapId);
                statement.ResultMap = resultMap ?? throw new SmartSqlException($"Statement.Id:{statement.Id} can not find ResultMap.Id:{resultMapId}");
            }

            string parameterMapId = statementNode.GetAttribute("ParameterMap");
            if (!String.IsNullOrEmpty(parameterMapId))
            {
                var parameterMap = smartSqlMap.ParameterMaps.FirstOrDefault(r => r.Id == parameterMapId);
                statement.ParameterMap = parameterMap ?? throw new SmartSqlException($"Statement.Id:{statement.Id} can not find ParameterMap.Id:{parameterMapId}");
            }
            #endregion
            var tagNodes = statementNode.Nodes();
            IList<Include> includes = new List<Include>();
            foreach (XNode tagNode in tagNodes)
            {
                var tag = LoadTag(tagNode, includes);
                if (tag != null) { statement.SqlTags.Add(tag); }
            }
            #region Init Include
            foreach (var include in includes)
            {
                if (include.RefId == statement.Id)
                {
                    throw new SmartSqlException($"Statement.Load Include.RefId can not be self statement.id:{include.RefId}");
                }
                var refStatement = smartSqlMap.Statements.FirstOrDefault(m => m.Id == include.RefId);

                include.Ref = refStatement ?? throw new SmartSqlException($"Statement.Load can not find statement.id:{include.RefId}");
            }
            #endregion
            return statement;
        }

        private ITag LoadTag(XNode xmlNode, IList<Include> includes)
        {
            ITag tag = null;
            var prepend = xmlNode?.GetAttribute("Prepend")?.Trim();
            var property = xmlNode?.GetAttribute("Property")?.Trim();
            var compareValue = xmlNode?.GetAttribute("CompareValue")?.Trim();
            #region Init Tag
            switch (xmlNode.GetName())
            {
                case "#text":
                case "#cdata-section":
                    {
                        var bodyText = " " + xmlNode.GetValue().Replace("\n", "").Trim();
                        return new SqlText
                        {
                            LineInfo = XmlLineInfo.Create(xmlNode),
                            BodyText = bodyText
                        };
                    }
                case "If":
                    {
                        tag = new IfTag
                        {
                            Test = xmlNode.GetAttribute("Test")
                        };
                        break;
                    }
                case "Include":
                    {
                        var refId = xmlNode?.GetAttribute("RefId");
                        var include_tag = new Include
                        {
                            RefId = refId,
                            Prepend = prepend
                        };
                        includes.Add(include_tag);
                        tag = include_tag;
                        break;
                    }
                case "IsEmpty":
                    {
                        tag = new IsEmpty
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }

                case "IsEqual":
                    {
                        tag = new IsEqual
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Bind":
                    {
                        tag = new BindTag
                        {
                            Name = xmlNode.GetAttribute("Name"),
                            Value = xmlNode.GetAttribute("Value"),
                        };
                        break;
                    }
                case "Trim":
                    {
                        tag = new TrimTag
                        {
                            Prefix = xmlNode.GetAttribute("Prefix"),
                            PrefixOverrides = xmlNode.GetAttribute("PrefixOverrides"),
                            Suffix = xmlNode.GetAttribute("Suffix"),
                            ChildTags = new List<ITag>(),
                        };
                        break;
                    }
                case "IsGreaterEqual":
                    {
                        tag = new IsGreaterEqual
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsGreaterThan":
                    {
                        tag = new IsGreaterThan
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsLessEqual":
                    {
                        tag = new IsLessEqual
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsLessThan":
                    {
                        tag = new IsLessThan
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsNotEmpty":
                    {
                        tag = new IsNotEmpty
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsNotEqual":
                    {
                        tag = new IsNotEqual
                        {
                            Prepend = prepend,
                            Property = property,
                            CompareValue = compareValue,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsNotNull":
                    {
                        tag = new IsNotNull
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsNull":
                    {
                        tag = new IsNull
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsTrue":
                    {
                        tag = new IsTrue
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsFalse":
                    {
                        tag = new IsFalse
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "IsProperty":
                    {
                        tag = new IsProperty
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Placeholder":
                    {
                        tag = new Placeholder
                        {
                            Prepend = prepend,
                            Property = property,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Switch":
                    {
                        tag = new Switch
                        {
                            Property = property,
                            Prepend = prepend,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Case":
                    {
                        var switchNode = xmlNode.Parent;
                        var switchProperty = xmlNode?.GetAttribute("Property")?.Trim();
                        var switchPrepend = xmlNode?.GetAttribute("Prepend")?.Trim();
                        tag = new Switch.Case
                        {
                            CompareValue = compareValue,
                            Property = switchProperty,
                            Prepend = switchPrepend,
                            Test = xmlNode?.GetAttribute("Test")?.Trim(),
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Default":
                    {
                        var switchNode = xmlNode.Parent;
                        var switchProperty = xmlNode?.GetAttribute("Property")?.Trim();
                        var switchPrepend = xmlNode?.GetAttribute("Prepend")?.Trim();
                        tag = new Switch.Defalut
                        {
                            Property = switchProperty,
                            Prepend = switchPrepend,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Dynamic":
                    {
                        tag = new Dynamic
                        {
                            Prepend = prepend,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Where":
                    {
                        tag = new Where
                        {
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Set":
                    {
                        tag = new Set
                        {
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "For":
                    {
                        var open = xmlNode?.GetAttribute("Open")?.Trim();
                        var separator = xmlNode?.GetAttribute("Separator")?.Trim();
                        var close = xmlNode?.GetAttribute("Close")?.Trim();
                        var key = xmlNode?.GetAttribute("Key")?.Trim();
                        tag = new For
                        {
                            Prepend = prepend,
                            Property = property,
                            Open = open,
                            Close = close,
                            Separator = separator,
                            Key = key,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "Env":
                    {
                        var dbProvider = xmlNode?.GetAttribute("DbProvider")?.Trim();
                        tag = new Env
                        {
                            Prepend = prepend,
                            DbProvider = dbProvider,
                            ChildTags = new List<ITag>()
                        };
                        break;
                    }
                case "#comment": { break; }
                default:
                    {
                        throw new SmartSqlException($"Statement.LoadTag unkonw tagName:{xmlNode.GetName()}.");
                    };
            }
            #endregion
            if (tag != null)
            {
                tag.LineInfo = XmlLineInfo.Create(xmlNode);
            }
            if (xmlNode is XElement ell)
            {
                foreach (XNode childNode in ell.Nodes())
                {
                    ITag childTag = LoadTag(childNode, includes);
                    if (childTag != null && tag != null)
                    {
                        childTag.Parent = tag;
                        (tag as Tag).ChildTags.Add(childTag);
                    }
                }
            }
            return tag;
        }
    }
}
