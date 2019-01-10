using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessDefinitionVariable
    {
        private string variableName;
        private string variableType;

        public ProcessDefinitionVariable()
        {
        }

        //[JsonConstructor]
        public ProcessDefinitionVariable([JsonProperty("VariableName")]string variableName,
            [JsonProperty("VariableType")]string variableType)
        {
            this.variableName = variableName;
            this.variableType = variableType;
        }

        public virtual string VariableName
        {
            get
            {
                return variableName;
            }
        }

        public virtual string VariableType
        {
            get
            {
                return variableType;
            }
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