using org.activiti.engine.impl.persistence.entity;

namespace org.activiti.engine.impl.bpmn.behavior
{
    public interface IMultiinstanceCompletedPolicy
    {
        string CompleteConditionVarName { get; set; }

        IExecutionEntity LeaveExection { get; set; }

        ITaskEntity CompleteTask { get; set; }

        bool CompletionConditionSatisfied(IExecutionEntity execution, MultiInstanceActivityBehavior multiInstanceActivity, object signalData);
    }
}
