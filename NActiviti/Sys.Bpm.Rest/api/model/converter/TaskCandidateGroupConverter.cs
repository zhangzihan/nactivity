using org.activiti.engine.task;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model.converter
{

    public class TaskCandidateGroupConverter : ModelConverter<IIdentityLink, TaskCandidateGroup>
    {
        private readonly ListConverter listConverter;

        public TaskCandidateGroupConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        public virtual TaskCandidateGroup from(IIdentityLink source)
        {
            TaskCandidateGroup taskCandidateGroup = null;

            if (source != null)
            {
                taskCandidateGroup = new TaskCandidateGroup(source.GroupId, source.TaskId);
            }
            return taskCandidateGroup;
        }

        public virtual IList<TaskCandidateGroup> from(IList<IIdentityLink> identityLinks)
        {
            return listConverter.from(identityLinks, this);
        }
    }

}