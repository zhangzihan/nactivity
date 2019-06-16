using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Sys.Workflow.Cloud.Services.Api.Model
{

    /// <summary>
    /// 流程服务任务
    /// </summary>
    public class ProcessDefinitionServiceTask
    {
        private string name;
        private string taskImplementation;


        /// <summary>
        /// 
        /// </summary>
        public ProcessDefinitionServiceTask()
        {
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">任务名称</param>
        /// <param name="implementation">实现</param>
        //[JsonConstructor]
        public ProcessDefinitionServiceTask([JsonProperty("Name")]string name,
            [JsonProperty("Implementation")]string implementation)
        {
            this.name = name;
            taskImplementation = implementation;
        }


        /// <summary>
        /// 任务名称
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
        /// 任务实现
        /// </summary>
        public virtual string TaskImplementation
        {
            get
            {
                return taskImplementation;
            }
            set => taskImplementation = value;
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