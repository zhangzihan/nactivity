using System;
using System.Collections.Generic;
using System.Text;

namespace org.activiti.bpmn.model
{
    public class FormField
    {
        public IList<FormFieldProperty> FieldProperties { get; } = new List<FormFieldProperty>();

        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
