using System;
using System.Collections.Generic;
using System.Text;

namespace Sys.Workflow.Bpmn.Models
{
    public class FormField
    {
        public IList<FormFieldProperty> FieldProperties { get; } = new List<FormFieldProperty>();

        public string Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}
