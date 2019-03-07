
import { IProcessDefinitionDeployerService } from './IProcessDefinitionDeployerService'
import Axios from 'axios';
import contants from 'contants';
import { IProcessDefinitionDeployer } from './IProcessDefinitionDeployer';
import { IResources } from 'model/query/IResources';

export class ProcessDefinitionDeployerService implements IProcessDefinitionDeployerService {

  private readonly serviceUrl: string = "/workflow/process-deployer";

  save(deployer: IProcessDefinitionDeployer): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}${this.serviceUrl}/save`, deployer).then(data => {
        res(data.data);
      });
    });
  }

  latest(query: any): Promise<IResources> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}${this.serviceUrl}/latest`, query).then(data => {
        res(data.data);
      });
    });
  }

  deploy(deployer: IProcessDefinitionDeployer): Promise<any> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}${this.serviceUrl}`, deployer).then(data => {
        res(data.data);
      });
    });
  }

  allDeployments(query: any): Promise<IResources> {
    return new Promise((res, rej) => {
      Axios.post(`${contants.serverUrl}${this.serviceUrl}/list`, query).then(data => {
        res(data.data);
      });
    });
  }

  remove(id: string): Promise<any> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}${this.serviceUrl}/${id}/remove`).then(data => {
        res(data.data);
      });
    });
  }

  getProcessModel(id: string): Promise<string> {
    return new Promise((res, rej) => {
      Axios.get(`${contants.serverUrl}${this.serviceUrl}/${id}/processmodel`).then(data => {
        res(data.data);
      });
    });
  }
}
