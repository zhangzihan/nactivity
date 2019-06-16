namespace Sys.Workflow.engine.@delegate.@event.impl
{

    /// 
    public class ActivitiSequenceFlowTakenEventImpl : ActivitiEventImpl, IActivitiSequenceFlowTakenEvent
    {

        protected internal string id;
        protected internal string sourceActivityId;
        protected internal string sourceActivityName;
        protected internal string sourceActivityType;
        protected internal string targetActivityId;
        protected internal string targetActivityName;
        protected internal string targetActivityType;
        protected internal string sourceActivityBehaviorClass;
        protected internal string targetActivityBehaviorClass;

        public ActivitiSequenceFlowTakenEventImpl(ActivitiEventType type) : base(type)
        {
        }

        public virtual string Id
        {
            get
            {
                return id;
            }
            set
            {
                this.id = value;
            }
        }


        public virtual string SourceActivityId
        {
            get
            {
                return sourceActivityId;
            }
            set
            {
                this.sourceActivityId = value;
            }
        }


        public virtual string SourceActivityName
        {
            get
            {
                return sourceActivityName;
            }
            set
            {
                this.sourceActivityName = value;
            }
        }


        public virtual string SourceActivityType
        {
            get
            {
                return sourceActivityType;
            }
            set
            {
                this.sourceActivityType = value;
            }
        }


        public virtual string TargetActivityId
        {
            get
            {
                return targetActivityId;
            }
            set
            {
                this.targetActivityId = value;
            }
        }


        public virtual string TargetActivityName
        {
            get
            {
                return targetActivityName;
            }
            set
            {
                this.targetActivityName = value;
            }
        }


        public virtual string TargetActivityType
        {
            get
            {
                return targetActivityType;
            }
            set
            {
                this.targetActivityType = value;
            }
        }


        public virtual string SourceActivityBehaviorClass
        {
            get
            {
                return sourceActivityBehaviorClass;
            }
            set
            {
                this.sourceActivityBehaviorClass = value;
            }
        }


        public virtual string TargetActivityBehaviorClass
        {
            get
            {
                return targetActivityBehaviorClass;
            }
            set
            {
                this.targetActivityBehaviorClass = value;
            }
        }


    }

}