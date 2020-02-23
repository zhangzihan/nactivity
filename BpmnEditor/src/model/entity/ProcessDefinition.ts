import contants from "contants";

let uuid = require('uuid');

export class ProcessDefinition {
  id: string;
  name: string;
  description: string;
  version: string;
  category: string;
  key: string;
  deploymentId: string;
  tenantId: string;
  businessKey: string;
  businessPath: string;
  startForm: string;
  xml: string;
  changed: boolean = false;

  static create(opts: {
    name: string,
    category?: string,
    businessKey?: string,
    businessPath?: string,
    startForm?: string,
    description?: string
  }): Promise<ProcessDefinition> {
    return new Promise<ProcessDefinition>((res, rej) => {
      var pdf = Object.assign(new ProcessDefinition(), opts);
      pdf.id = uuid();
      var model = contants.defaultBpmXml(opts.name);
      pdf.key = model.key;
      pdf.xml = model.xml;
      pdf.version = "0";
      res(pdf);
    });
  }
}
