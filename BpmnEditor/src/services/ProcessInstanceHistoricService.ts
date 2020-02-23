import { Settings } from 'model/settings';
import { IHistoricInstance } from 'model/resource/IHistoricInstance';
import { IHistoricInstanceQuery } from 'model/query/IHistoricInstanceQuery';
import { HttpInvoker } from 'services/httpInvoker';
import { IProcessInstanceService } from "./IProcessInstanceService";
import contants from "../contants";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";
import { IProcessInstanceQuery } from 'model/query/IProcessInstanceQuery'
import { IProcessInstanceTaskQuery } from "model/query/IProcessInstanceTaskQuery";
import { inject, singleton, autoinject } from "aurelia-framework";
import { IProcessInstanceAdminService } from './IProcessInstanceAdminService';
import { IProcessInstanceHistoriceService } from './IProcessInstanceHistoriceService';
import { LoginUser } from 'model/loginuser';

@autoinject()
@singleton()
export class ProcessInstanceHistoriceService implements IProcessInstanceHistoriceService {

  private controller = "history/process-instances";

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  processInstances(query: IHistoricInstanceQuery): Promise<IResources<IHistoricInstance>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  getProcessInstanceById(processInstanceId: string): Promise<IHistoricInstance> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${processInstanceId}`)).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
