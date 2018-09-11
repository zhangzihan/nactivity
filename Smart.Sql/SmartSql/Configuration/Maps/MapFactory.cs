using SmartSql.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SmartSql.Configuration.Maps
{
    public class MapFactory
    {
        public static ResultMap LoadResultMap(XElement xmlNode, SmartSqlMapConfig sqlMapConfig)
        {
            ResultMap resultMap = new ResultMap
            {
                Id = xmlNode.Attribute("Id").Value,
                Results = new List<Result> { }
            };
            var type = xmlNode.Attribute("ResultType")?.Value;
            if (string.IsNullOrWhiteSpace(type) == false)
            {
                resultMap.ResultType = Type.GetType(type);
            }
            if (resultMap.ResultType == null)
            {
                throw new SmartSqlException($"Entity map {resultMap.Id} 找不到类型{resultMap.ResultType}!");
            }
            foreach (XElement childNode in xmlNode.Elements())
            {
                var result = new Result
                {
                    Property = childNode.Attribute("Property").Value,
                    Column = (childNode.Attribute("Column") ?? childNode.Attribute("Property")).Value,
                    TypeHandler = childNode.Attribute("TypeHandler")?.Value
                };
                result.Handler = sqlMapConfig.TypeHandlers.FirstOrDefault(th => th.Name == result.TypeHandler)?.Handler;
                resultMap.Results.Add(result);
            }
            return resultMap;
        }
        public static ParameterMap LoadParameterMap(XElement xmlNode, SmartSqlMapConfig sqlMapConfig)
        {
            ParameterMap parameterMap = new ParameterMap
            {
                Id = xmlNode.Attribute("Id").Value,
                Parameters = new List<Parameter> { }
            };

            foreach (XElement childNode in xmlNode.Elements())
            {
                var parameter = new Parameter
                {
                    Property = childNode.Attribute("Property").Value,
                    Name = (childNode.Attribute("Name") ?? childNode.Attribute("Property")).Value,
                    TypeHandler = childNode.Attribute("TypeHandler")?.Value
                };
                parameter.Handler = sqlMapConfig.TypeHandlers.FirstOrDefault(th => th.Name == parameter.TypeHandler)?.Handler;
                parameterMap.Parameters.Add(parameter);
            }

            return parameterMap;
        }
    }
}
