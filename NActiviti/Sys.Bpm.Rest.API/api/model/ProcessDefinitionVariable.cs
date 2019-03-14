using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{

    /// <summary>
    /// 流程变量
    /// </summary>
    public class ProcessDefinitionVariable
    {
        private string variableName;
        private string variableType;

        /// <summary>
        /// 
        /// </summary>

        public ProcessDefinitionVariable()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="variableName">变量名</param>
        /// <param name="variableType">变量类型</param>
        //[JsonConstructor]
        public ProcessDefinitionVariable([JsonProperty("VariableName")]string variableName,
            [JsonProperty("VariableType")]string variableType)
        {
            this.variableName = variableName;
            this.variableType = variableType;
        }

        /// <summary>
        /// 变量名
        /// </summary>
        public virtual string VariableName
        {
            get
            {
                return variableName;
            }
            set => variableName = value;
        }

        /// <summary>
        /// 变量类型
        /// </summary>

        public virtual string VariableType
        {
            get
            {
                return variableType;
            }
            set => variableType = value;
        }

        //public override ISet<ProcessDefinitionVariable> deserialize(JsonParser jp, DeserializationContext ctxt)
        //{

        //    ISet<ProcessDefinitionVariable> variables = new HashSet<ProcessDefinitionVariable>();
        //    ObjectCodec oc = jp.Codec;
        //    JsonNode nodes = oc.readTree(jp);

        //    for (int i = 0; i < nodes.size(); i++)
        //    {
        //        ProcessDefinitionVariable variable = new ProcessDefinitionVariable(nodes.get(i).get("variableName").asText(), nodes.get(i).get("variableType").asText());
        //        variables.Add(variable);
        //    }

        //    return variables;
        //}
    }

}