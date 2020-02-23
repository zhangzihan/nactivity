
let uuid = require('uuid');

export class ProcessDefinitionDeployer {
  id: string;
  disableSchemaValidation: boolean = true;
  disableBpmnValidation: boolean = false;
  name: string;
  category: string;
  key: string;
  tenantId: string;
  enableDuplicateFiltering: boolean = true;
  bpmnXML: string;
  businessKey: string;
  businessPath: string;
  startForm: string;
  processDefinitions = [];

  static create(opts: { name: string }): Promise<ProcessDefinitionDeployer> {
    return new Promise<ProcessDefinitionDeployer>((res, rej) => {
      var dep = Object.assign(new ProcessDefinitionDeployer(), opts);
      dep.id = uuid();
      return res(dep);
    });
  }
}
