import { WorkflowDocument } from './../WorkflowDocument';
import { inject } from "aurelia-framework";
import { IProcessInstanceService } from '../services/IProcessInstanceService'
import { DirectionEnum } from "model/query/ISort";
import { EventBus } from "EventBus";
import { activationStrategy, Router } from 'aurelia-router';
import { IProcessInstanceAdminService } from 'services/IProcessInstanceAdminService';
import { IProcessInstanceHistoriceService } from 'services/IProcessInstanceHistoriceService';

@inject('processInstanceService', 'processInstanceAdminService', 'historyService', 'eventBus', 'document', Router)
export class ProcInstancesViewModel {

  instances: Array<any>;

  procName: string;

  status: string = "uncompleted";

  constructor(private procInsSrv: IProcessInstanceService,
    private adminService: IProcessInstanceAdminService,
    private historyService: IProcessInstanceHistoriceService,
    private es: EventBus,
    private doc: WorkflowDocument,
    private router: Router) {

  }

  activate(model, nctx) {
    this.procName = this.doc.workflow.name;

    this.unCompleted();

    this.es.subscribe("openWorkflow", (doc) => {
      this.procName = doc.name;
      this.load();
    })
  }

  load() {
    if (this.status == 'uncompleted') {
      this.unCompleted();
    } else {
      this.completed();
    }
  }

  determineActivationStrategy() {
    return activationStrategy.replace;
  }

  completed() {
    this.status = "completed";
    this.historyService.processInstances({
      processDefinitionName: this.procName,
      pageable: {
        pageNo: 1,
        pageSize: 1000,
      }
    }).then((data: any) => {
      this.instances = data;
    });
  }

  unCompleted() {
    this.status = "uncompleted";
    this.procInsSrv.processInstances({
      onlyProcessInstances: true,
      processDefinitionName: this.procName,
      pageable: {
        pageNo: 1,
        pageSize: 100,
        sort: [
          {
            property: "startDate",
            direction: DirectionEnum.desc
          }
        ]
      }
    }).then((data: any) => {
      this.instances = data;
    })
  }

  attached() {

  }

  detail(id) {
    this.router.navigateToRoute("instance", { id: id, status: this.status });
  }

  terminate(id) {
    this.procInsSrv.terminate(id, "已终止整个流程实例").then(data => {
      this.load();
    }).catch(err => {
      alert(err);
    });
  }
}
