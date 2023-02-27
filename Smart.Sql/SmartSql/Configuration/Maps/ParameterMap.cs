using SmartSql.Abstractions.TypeHandler;
using Spring.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmartSql.Configuration.Maps
{
	public class ParameterMap
	{
		private string id;

		public string Id { get => id; set => id = StringUtils.PoolString(value); }
		public IList<Parameter> Parameters { get; set; }
	}
	public class Parameter
	{
		private string property;
		private string name;
		private string typeHandler;

		public string Property { get => property; set => property = StringUtils.PoolString(value); }
		public string Name { get => name; set => name = StringUtils.PoolString(value); }
		public string TypeHandler { get => typeHandler; set => typeHandler = StringUtils.PoolString(value); }
		public ITypeHandler Handler { get; set; }
	}
}
