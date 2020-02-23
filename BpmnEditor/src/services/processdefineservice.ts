import { HttpInvoker } from 'services/httpInvoker';
import Axios from 'axios';
import contants from '../contants';
import { inject, singleton, autoinject } from 'aurelia-framework';
import { LoginUser } from 'model/loginuser';
import { Settings } from 'model/settings';

@autoinject()
@singleton()
export class ProcessDefineService implements IProcessDefineService {

  private controller = 'workflow/process-definitions';

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  latest(query: any): Promise<any> {
    query.tenantId = this.loginUser.current.tenantId;
    query.pageable = {
      Sort: [{
        property: "name"
      }]
    }
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/latest`), query).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  getProcessModel(id): Promise<string> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${id}/processmodel`)).then(data => {
        return res(data.data);
      }).catch(err => rej(err));
    });
  }

  processDefinitions(query: any): Promise<any> {
    query.tenantId = this.loginUser.current.tenantId;
    query.pageable = Object.assign({}, query.pageable);
    if (query.pageable.sort == null) {
      query.pageable = {
        sort: [{
          property: "name"
        }]
      }
    }
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(this.controller + '/list'), query).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
