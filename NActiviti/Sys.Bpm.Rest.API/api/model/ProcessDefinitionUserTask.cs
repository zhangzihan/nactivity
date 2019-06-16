using Newtonsoft.Json;
using System.Collections.Generic;

namespace Sys.Workflow.Cloud.Services.Api.Model
{

    /// <summary>
    /// 用户任务
    /// </summary>
    public class ProcessDefinitionUserTask
    {
        private string name;
        private string taskDocumentation;


        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionUserTask()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="documentation">描述</param>
        //[JsonConstructor]
        public ProcessDefinitionUserTask([JsonProperty("Name")]string name,
            [JsonProperty("Description")]string documentation)
        {
            this.name = name;
            taskDocumentation = documentation;
        }

        /// <summary>
        /// 名称
        /// </summary>

        public virtual string Name
        {
            get
            {
                return name;
            }
            set => name = value;
        }

        /// <summary>
        /// 描述
        /// </summary>

        public virtual string TaskDocumentation
        {
            get
            {
                return taskDocumentation;
            }
            set => taskDocumentation = value;
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