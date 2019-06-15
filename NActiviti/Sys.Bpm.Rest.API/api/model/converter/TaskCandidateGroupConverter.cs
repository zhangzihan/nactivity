using org.activiti.engine.task;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model.converter
{

    /// <summary>
    /// 
    /// </summary>

    public class TaskCandidateGroupConverter : IModelConverter<IIdentityLink, TaskCandidateGroup>
    {
        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateGroupConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual TaskCandidateGroup From(IIdentityLink source)
        {
            TaskCandidateGroup taskCandidateGroup = null;

            if (source != null)
            {
                taskCandidateGroup = new TaskCandidateGroup(source.GroupId, source.TaskId);
            }
            return taskCandidateGroup;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual IEnumerable<TaskCandidateGroup> From(IEnumerable<IIdentityLink> identityLinks)
        {
            return listConverter.From(identityLinks, this);
        }
    }

}