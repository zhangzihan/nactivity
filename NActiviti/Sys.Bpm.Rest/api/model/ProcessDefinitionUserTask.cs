using Newtonsoft.Json;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model
{
    public class ProcessDefinitionUserTask
    {
        private string taskName;
        private string taskDocumentation;

        public ProcessDefinitionUserTask()
        {
        }

        [JsonConstructor]
        public ProcessDefinitionUserTask([JsonProperty("Name")]string name,
            [JsonProperty("Description")]string documentation)
        {
            taskName = name;
            taskDocumentation = documentation;
        }

        public virtual string TaskName
        {
            get
            {
                return taskName;
            }
        }

        public virtual string TaskDocumentation
        {
            get
            {
                return taskDocumentation;
            }
        }


        //public override ISet<ProcessDefinitionUserTask> deserialize(JsonParser jp, DeserializationContext ctxt)
        //{

        //    ISet<ProcessDefinitionUserTask> tasks = new HashSet<ProcessDefinitionUserTask>();
        //    ObjectCodec oc = jp.Codec;
        //    JsonNode nodes = oc.readTree(jp);

        //    for (int i = 0; i < nodes.size(); i++)
        //    {
        //        ProcessDefinitionUserTask task = new ProcessDefinitionUserTask(nodes.get(i).get("taskName").asText(), nodes.get(i).get("taskDocumentation").asText());
        //        tasks.Add(task);
        //    }

        //    return tasks;
        //}
    }

}