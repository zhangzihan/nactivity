import { HttpInvoker } from 'services/httpInvoker';
import { IProcessInstanceService } from "./IProcessInstanceService";
import contants from "../contants";
import Axios from "axios";
import { IProcessInstanceQuery } from "model/query/IProcessInstanceQuery";
import { IStartProcessInstanceCmd } from "model/cmd/IStartProcessInstanceCmd";
import { IResources } from "model/query/IResources";
import { IProcessInstance } from "model/resource/IProcessInstance";
import { IProcessInstanceTaskQuery } from "model/query/IProcessInstanceTaskQuery";
import { ITaskModel } from "model/resource/ITaskModel";
import { inject, singleton, autoinject } from "aurelia-framework";
import { LoginUser } from 'model/loginuser';
import { Settings } from 'model/settings';

@autoinject()
@singleton()
export class ProcessInstanceServie implements IProcessInstanceService {

  private controller = "workflow/process-instances";

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  getTasks(processInstanceId: string, query: IProcessInstanceTaskQuery): Promise<IResources<ITaskModel>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/tasks`), query).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  processInstances(query: IProcessInstanceQuery): Promise<IResources<IProcessInstance>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  start(cmd: Array<IStartProcessInstanceCmd>): Promise<Array<IProcessInstance>> {
    cmd.forEach(c => {
      c.variables = Object.assign({
        debugMode: true
      }, c.variables);
      c.tenantId = this.loginUser.current.tenantId
    });
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/start`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  getProcessInstanceById(processInstanceId: string): Promise<IProcessInstance> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${processInstanceId}`)).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  getProcessDiagram(processInstanceId: string): Promise<string> {
    throw new Error("Method not implemented.");
  }

  sendSignal(cmd: any): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/signal`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  suspend(processInstanceId: string): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${processInstanceId}/suspend`)).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  activate(processInstanceId: string): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${processInstanceId}/activate`)).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  terminate(processInstanceId: string, reason: string): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/terminate`), {
        processInstanceId: processInstanceId,
        reason: reason
      }).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
