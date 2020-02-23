import { ITaskAdminService } from './ITaskAdminService';
import { autoinject, singleton } from 'aurelia-framework';
import { HttpInvoker } from './httpInvoker';
import contants from 'contants';
import { LoginUser } from 'model/loginuser';
import { Settings } from 'model/settings';

@autoinject()
@singleton()
export class TaskAdminService implements ITaskAdminService {

  private controller = 'workflow/admin/tasks';

  constructor(private httpInvoker: HttpInvoker, private loginUser: LoginUser, private settings: Settings) {

  }

  getAllTasks(query: import("../model/query/ITaskQuery").ITaskQuery): Promise<any> {
    query.tenantId = this.loginUser.current.tenantId
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  reassignTaskUser(cmd: any): Promise<any> {
    cmd.tenantId = this.loginUser.current.tenantId
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/reassign`), cmd).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }
}
