using System.Collections.Generic;

namespace org.activiti.services.api.commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface IStartProcessInstanceCmd
    {
        /// <summary>
        /// 
        /// </summary>
        string BusinessKey { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        string ProcessDefinitionId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ProcessDefinitionKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string ProcessInstanceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string StartForm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        string TenantId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<string, object> Variables { get; set; }
    }
}