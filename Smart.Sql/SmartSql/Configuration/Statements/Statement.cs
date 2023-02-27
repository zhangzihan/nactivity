using SmartSql.Abstractions;
using SmartSql.Abstractions.Cache;
using SmartSql.Configuration.Maps;
using SmartSql.Configuration.Tags;
using Spring.Util;
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SmartSql.Configuration.Statements
{
    public enum StatementType
    {
        Statement,
        Insert,
        Update,
        Delete,
        Select
    }

	public class Statement
	{
		private string id;

		public virtual StatementType Type { get { return StatementType.Statement; } }
		[XmlIgnore]
		public SmartSqlMap SmartSqlMap { get; internal set; }
		[XmlAttribute]
		public string Id { get => id; set => id = StringUtils.PoolString(value); }
		public string FullSqlId => $"{SmartSqlMap.Scope}.{Id}";
		public IList<ITag> SqlTags { get; set; }
		public Cache Cache { get; set; }
		public ResultMap ResultMap { get; set; }
		public Type ResultType { get; set; }
		public ParameterMap ParameterMap { get; set; }

		public void BuildSql(RequestContext context)
		{
			foreach (ITag tag in SqlTags)
			{
				try
				{
					tag.BuildSql(context);
				}
				catch (BuildSqlException e)
				{
					throw new BuildSqlException($"构造SQL语句发生错误{FullSqlId}:({tag.LineInfo.LineNumber},{tag.LineInfo.LinePosition}):{e.Message}", e);
				}
			}
		}
	}
}
