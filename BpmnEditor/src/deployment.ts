import { DirectionEnum } from 'model/query/ISort';
import { IProcessDefinitionDeployerService } from './services/IProcessDefinitionDeployerService';
import { useView, PLATFORM, inject, observable } from "aurelia-framework";
import { EventAggregator } from 'aurelia-event-aggregator';
import { EventBus } from 'EventBus';

@inject('processDefinitionDeployer', 'eventBus')
export class DeploymentViewModel {

  deployments: Array<any>;

  @observable select: any;

  constructor(private deployService: IProcessDefinitionDeployerService, private es: EventBus) {
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
        pageNo: 1,
        pageSize: 1000,
        sort: [{
          property: 'name',
          direction: DirectionEnum.asc
        }]
      }
    }).then(data => this.deployments = data.list);
  }

  drafts() {
    this.deployService.allDeployments({
      onlyDrafts: true,
      pageable: {
        pageNo: 1,
        pageSize: 1000,
        sort: [{
          property: 'name',
          direction: DirectionEnum.asc
        }]
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
        pageNo: 1,
        pageSize: 1000,
        sort: [{
          property: 'name',
          direction: DirectionEnum.asc
        }]
      }
    }).then(data => this.deployments = data.list);
  }
}
