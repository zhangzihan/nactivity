using System;

namespace Sys.Workflow.Services.Api.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUpdateTaskCmd
    {
        /// <summary>
        /// 
        /// </summary>
        string TaskId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Assignee { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        DateTime? DueDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ParentTaskId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        int? Priority { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string TenantId { get; set; }
    }
}