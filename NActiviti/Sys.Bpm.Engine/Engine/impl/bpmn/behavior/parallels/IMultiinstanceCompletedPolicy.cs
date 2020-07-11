using Sys.Workflow.Engine.Impl.Persistence.Entity;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMultiinstanceCompletedPolicy
    {
        /// <summary>
        /// 
        /// </summary>
        string CompleteConditionVarName { get; set; }
        /// <summary>
        /// 
        /// </summary>
        IExecutionEntity LeaveExection { get; set; }
        /// <summary>
        /// 
        /// </summary>
        ITaskEntity CompleteTask { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="execution"></param>
        /// <param name="multiInstanceActivity"></param>
        /// <param name="signalData"></param>
        /// <returns></returns>
        bool CompletionConditionSatisfied(IExecutionEntity execution, MultiInstanceActivityBehavior multiInstanceActivity, object signalData);
    }
}
