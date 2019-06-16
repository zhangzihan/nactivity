using System.Collections.Generic;

namespace Sys.Workflow.Bpmn.Models
{

    public class DataAssociation : BaseElement
    {

        protected internal string sourceRef;
        protected internal string targetRef;
        protected internal string transformation;
        protected internal IList<Assignment> assignments = new List<Assignment>();

        public virtual string SourceRef
        {
            get
            {
                return sourceRef;
            }
            set
            {
                this.sourceRef = value;
            }
        }


        public virtual string TargetRef
        {
            get
            {
                return targetRef;
            }
            set
            {
                this.targetRef = value;
            }
        }


        public virtual string Transformation
        {
            get
            {
                return transformation;
            }
            set
            {
                this.transformation = value;
            }
        }


        public virtual IList<Assignment> Assignments
        {
            get
            {
                return assignments;
            }
            set
            {
                this.assignments = value;
            }
        }


        public override BaseElement Clone()
        {
            DataAssociation clone = new DataAssociation
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                var val = value as DataAssociation;

                SourceRef = val.SourceRef;
                TargetRef = val.TargetRef;
                Transformation = val.Transformation;

                assignments = new List<Assignment>();
                if (val.Assignments != null && val.Assignments.Count > 0)
                {
                    foreach (Assignment assignment in val.Assignments)
                    {
                        assignments.Add(assignment.Clone() as Assignment);
                    }
                }
            }
        }
    }

}