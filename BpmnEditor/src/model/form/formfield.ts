import { WorkflowParameter } from "model/entity/WorkflowParameter";

export class FormField {
  id: string;
  name: string;
  type: string;
  value: any;

  constructor(id, name, type, value?) {
    this.id = id;
    this.name = name;
    this.type = type;
    this.value = value;
  }

  static fromWorkflowParameter(param: WorkflowParameter): Array<FormField> {
    let fields = [];
    if (param.businessKey != null) {
      let desc = (param.businessKey.descriptor || { name: "businessKey", type: "string" });
      fields.push(new FormField("businessKey", desc.name, desc.type, param.businessKey.value));
    }

    param.variables.forEach(v => {
      let desc = v.descriptor || { name: v.id, type: "string" };
      let fld: FormField = new FormField(v.id, desc.name, desc.type, v[v.id]);
      fields.push(fld);
    });

    return fields;
  }
}
