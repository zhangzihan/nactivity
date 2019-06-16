using Sys.Workflow.Engine.Impl.Persistence.Entity;

namespace Sys.Workflow.Engine.Impl.Bpmn.Behavior
{
    public interface IMultiinstanceCompletedPolicy
    {
        string CompleteConditionVarName { get; set; }

        IExecutionEntity LeaveExection { get; set; }

        ITaskEntity CompleteTask { get; set; }

        bool CompletionConditionSatisfied(IExecutionEntity execution, MultiInstanceActivityBehavior multiInstanceActivity, object signalData);
    }
}
