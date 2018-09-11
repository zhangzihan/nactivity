using org.activiti.engine.task;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model.converter
{

    public class TaskCandidateUserConverter : ModelConverter<IIdentityLink, TaskCandidateUser>
    {

        private readonly ListConverter listConverter;

        public TaskCandidateUserConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        public virtual TaskCandidateUser from(IIdentityLink source)
        {
            TaskCandidateUser taskCandidateUser = null;

            if (source != null)
            {
                taskCandidateUser = new TaskCandidateUser(source.UserId, source.TaskId);
            }
            return taskCandidateUser;
        }

        public virtual IList<TaskCandidateUser> from(IList<IIdentityLink> identityLinks)
        {
            return listConverter.from(identityLinks, this);
        }
    }

}