import { IHistoricInstance } from 'model/resource/IHistoricInstance';
import { IHistoricInstanceQuery } from 'model/query/IHistoricInstanceQuery';
import { HttpInvoker } from 'services/httpInvoker';
import { IProcessInstanceService } from "./IProcessInstanceService";
import contants from "contants";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";
import { IProcessInstanceQuery } from 'model/query/IProcessInstanceQuery'
import { IProcessInstanceTaskQuery } from "model/query/IProcessInstanceTaskQuery";
import { inject } from "aurelia-framework";
import { IProcessInstanceAdminService } from './IProcessInstanceAdminService';
import { IProcessInstanceHistoriceService } from './IProcessInstanceHistoriceService';

@inject('httpInvoker')
export class ProcessInstanceHistoriceService implements IProcessInstanceHistoriceService {

  private controller = "history/process-instances";

  constructor(private httpInvoker: HttpInvoker) {

  }

  processInstances(query: IHistoricInstanceQuery): Promise<IResources<IHistoricInstance>> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(`${contants.serverUrl}/${this.controller}`, query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  getProcessInstanceById(processInstanceId: string): Promise<IHistoricInstance> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(`${contants.serverUrl}/${this.controller}/${processInstanceId}`).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
