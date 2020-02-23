
import { IProcessDefinitionDeployerService } from './IProcessDefinitionDeployerService'
import Axios from 'axios';
import contants from '../contants';
import { IProcessDefinitionDeployer } from './IProcessDefinitionDeployer';
import { IResources } from 'model/query/IResources';
import { HttpInvoker } from './httpInvoker';
import { inject, singleton, autoinject } from 'aurelia-framework';
import { LoginUser } from 'model/loginuser';
import { Settings } from 'model/settings';

@autoinject()
@singleton()
export class ProcessDefinitionDeployerService implements IProcessDefinitionDeployerService {

  private readonly controller: string = "workflow/process-deployer";

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  save(deployer: IProcessDefinitionDeployer): Promise<any> {
    deployer.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(this.controller + '/save'), deployer).then(data => {
        res(data.data);
      });
    });
  }

  latest(query: any): Promise<IResources<any>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/latest`), query).then(data => {
        res(data.data);
      });
    });
  }

  draft(name): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${this.loginUser.current.tenantId}/${name}/draft`)).then(data => {
        if (data.status == 204) {
          return res(null);
        }
        return res(data.data);
      });
    });
  }

  deploy(deployer: IProcessDefinitionDeployer): Promise<any> {
    deployer.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), deployer).then(data => {
        res(data.data);
      }).catch(r => rej(r));
    });
  }

  allDeployments(query: any): Promise<IResources<any>> {
    query.tenantId = this.loginUser.current.tenantId;
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/list`), query).then(data => {
        res(data.data);
      });
    });
  }

  remove(id: string): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${id}/remove`)).then(data => {
        res(data.data);
      });
    });
  }

  getProcessModel(id: string): Promise<string> {
    return new Promise((res, rej) => {
      this.httpInvoker.get(this.settings.getUrl(`${this.controller}/${id}/processmodel`)).then(data => {
        res(data.data);
      });
    });
  }
}
