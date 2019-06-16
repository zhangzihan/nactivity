﻿using Sys.Workflow.engine.task;
using System.Collections.Generic;

namespace Sys.Workflow.cloud.services.api.model.converter
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

        public virtual TaskCandidateUser From(IIdentityLink source)
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
        public virtual IEnumerable<TaskCandidateUser> From(IEnumerable<IIdentityLink> identityLinks)
        {
            return listConverter.From(identityLinks, this);
        }
    }

}