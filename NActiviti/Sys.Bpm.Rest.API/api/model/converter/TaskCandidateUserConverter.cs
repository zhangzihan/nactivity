using org.activiti.engine.task;
using System.Collections.Generic;

namespace org.activiti.cloud.services.api.model.converter
{


    /// <summary>
    /// 
    /// </summary>
    public class TaskCandidateUserConverter : IModelConverter<IIdentityLink, TaskCandidateUser>
    {

        private readonly ListConverter listConverter;


        /// <summary>
        /// 
        /// </summary>
        public TaskCandidateUserConverter(ListConverter listConverter)
        {
            this.listConverter = listConverter;
        }

        /// <summary>
        /// 
        /// </summary>

        public virtual TaskCandidateUser from(IIdentityLink source)
        {
            TaskCandidateUser taskCandidateUser = null;

            if (source != null)
            {
                taskCandidateUser = new TaskCandidateUser(source.UserId, source.TaskId);
            }
            return taskCandidateUser;
        }


        /// <summary>
        /// 
        /// </summary>
        public virtual IList<TaskCandidateUser> from(IList<IIdentityLink> identityLinks)
        {
            return listConverter.from(identityLinks, this);
        }
    }

}