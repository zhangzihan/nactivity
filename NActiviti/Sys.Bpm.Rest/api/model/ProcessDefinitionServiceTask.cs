using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessDefinitionServiceTask
    {
        private string taskName;
        private string taskImplementation;

        public ProcessDefinitionServiceTask()
        {
        }

        [JsonConstructor]
        public ProcessDefinitionServiceTask([JsonProperty("Name")]string name,
            [JsonProperty("Implementation")]string implementation)
        {
            taskName = name;
            taskImplementation = implementation;
        }

        public virtual string TaskName
        {
            get
            {
                return taskName;
            }
        }

        public virtual string TaskImplementation
        {
            get
            {
                return taskImplementation;
            }
        }

        //public override ISet<ProcessDefinitionServiceTask> deserialize(JsonParser jp, DeserializationContext ctxt)
        //{

        //    ISet<ProcessDefinitionServiceTask> tasks = new HashSet<ProcessDefinitionServiceTask>();
        //    ObjectCodec oc = jp.Codec;
        //    JsonNode nodes = oc.readTree(jp);

        //    for (int i = 0; i < nodes.size(); i++)
        //    {
        //        ProcessDefinitionServiceTask task = new ProcessDefinitionServiceTask(nodes.get(i).get("taskName").asText(), nodes.get(i).get("taskImplementation").asText());
        //        tasks.Add(task);
        //    }

        //    return tasks;
        //}
    }

}