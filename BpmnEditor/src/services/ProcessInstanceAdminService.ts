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
import { LoginUser } from 'model/loginuser';

@autoinject()
@singleton()
export class ProcessInstanceAdminService implements IProcessInstanceAdminService {

  private controller = "workflow/admin/process-instances";

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  getAllProcessInstances(query: IProcessInstanceQuery): Promise<IResources<IProcessInstance>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  /**
  /// <summary>
  /// 获取所有历史流程实例
  /// </summary>
  /// <param name="query">流程实例查询对象</param>
  /// <returns>Task < Resources < HistoricInstance >> </returns>
   */
  getAllProcessHistoriecs(query: IHistoricInstanceQuery): Promise<IResources<IHistoricInstance>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/historices`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }
}
