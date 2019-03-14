import { HttpInvoker } from 'services/httpInvoker';
import Axios from 'axios';
import contants from 'contants';
import { inject } from 'aurelia-framework';

@inject('httpInvoker')
export class ProcessDefineService implements IProcessDefineService {

  private controller = 'workflow/process-definitions';

  constructor(private httpInvoker: HttpInvoker) {

  }

  latest(query: any): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(`${contants.serverUrl}/${this.controller}/latest`, query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  getProcessModel(id): Promise<string> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(`${contants.serverUrl}/${this.controller}/${id}/processmodel`).then(data => {
        return res(data.data);
      }).catch(err => rej(err));
    });
  }

}
