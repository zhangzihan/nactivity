import Axios from 'axios';
import contants from 'contants';

export class ProcessDefineService implements IProcessDefineService {

  private controller = 'workflow/process-definitions';

  latest(query: any): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}/${this.controller}/latest`, query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  getProcessModel(id): Promise<string> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}/${this.controller}/${id}/processmodel`).then(data => {
        return res(data.data);
      }).catch(err => rej(err));
    });
  }

}
