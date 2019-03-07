import { IProcessDefinitionDeployerService } from './services/IProcessDefinitionDeployerService';
import { useView, PLATFORM, inject, observable } from "aurelia-framework";
import { EventAggregator } from 'aurelia-event-aggregator';

@useView(PLATFORM.moduleName('./deployment.html'))
@inject('processDefinitionDeployer', EventAggregator)
export class DeploymentViewModel {

  deployments: Array<any>;

  @observable select: any;

  constructor(private deployService: IProcessDefinitionDeployerService, private es: EventAggregator) {
    this.allDeployments();
  }

  selected(id) {
    this.select = this.deployments.find(x => x.id == id);
    this.deployService.getProcessModel(id).then(xml => {
      this.select.xml = xml;
      this.es.publish("openWorkflow", this.select);
    })
  }

  allDeployments() {
    this.deployService.allDeployments({
      pageable: {
        offset: 0,
        pageSize: 100
      }
    }).then(data => this.deployments = data.list);
  }

  remove() {
    if (this.select != null) {
      this.deployService.remove(this.select.id).then(r => {
        var idx = this.deployments.findIndex(x => x.id == this.select.id);
        if (idx > -1) {
          this.deployments.splice(idx, 1);
        }
        this.select = null;
      }).catch(err => alert(err));
    }
  }

  addDeployment() {

  }

  latest() {
    this.deployService.latest({
      pageable: {
        offset: 0,
        pageSize: 1000
      }
    }).then(data => this.deployments = data.list);
  }
}
