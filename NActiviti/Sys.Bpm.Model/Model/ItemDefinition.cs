using System;
namespace Sys.Workflow.Bpmn.Models
{
    public class ItemDefinition : BaseElement
    {

        protected internal string structureRef;
        protected internal string itemKind;

        public virtual string StructureRef
        {
            get
            {
                return structureRef;
            }
            set
            {
                this.structureRef = value;
            }
        }


        public virtual string ItemKind
        {
            get
            {
                return itemKind;
            }
            set
            {
                this.itemKind = value;
            }
        }


        public override BaseElement Clone()
        {
            ItemDefinition clone = new ItemDefinition
            {
                Values = this
            };
            return clone;
        }

        public override BaseElement Values
        {
            set
            {
                base.Values = value;
                if (value is not ItemDefinition val) throw new ArgumentException($"Expected {nameof(ItemDefinition)}, got {value?.GetType().Name}", nameof(value));

                StructureRef = val.StructureRef;
                ItemKind = val.ItemKind;
            }
        }
    }

}