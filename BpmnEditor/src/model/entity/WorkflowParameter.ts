
export class WorkflowParameter {
  businessKey?: { value: any, descriptor?: WorkflowParameterDescriptor };
  variables: Array<{ id: string, [id: string]: any, descriptor?: WorkflowParameterDescriptor }> = [];

  static fromJSON(data): WorkflowParameter {
    let wp = new WorkflowParameter();

    if (data.businessKey) {
      wp.businessKey = {
        value: data.businessKey.value,
        descriptor: WorkflowParameterDescriptor.fromJSON(data.businessKey.descriptor)
      }
    }

    if (data.variables) {
      data.variables.forEach(v => {
        let keys = Object.keys(v);
        let obj: any = {};
        keys.forEach(key => {
          if (key == "descriptor") {
            obj[key] = WorkflowParameterDescriptor.fromJSON(v[key]);
          } else {
            obj[key] = v[key];
            obj.id = key
          }
        });
        wp.variables.push(obj);
      });
    }

    return wp;
  }
}

export class WorkflowParameterDescriptor {
  name: string;
  type?: string;

  static fromJSON(data): WorkflowParameterDescriptor {
    let wpd = new WorkflowParameterDescriptor;
    if (data == null) {
      return wpd;
    }
    wpd.name = data.name;
    wpd.type = data.type;

    return wpd;
  }
}

export let workflowParameterDescriptor =
  `{
  "businessKey": {
    "value": "业务ID,提交到流程的变量值",
    "descriptor": {
      "name": "业务ID",
      "type": "string"
    }
  },
  "variables": [
    {
      "变量名":"变量值",
      "descriptor": {
        "name": "变量名说明",
        "type": "string"
      }
    }
  ]
}`;
