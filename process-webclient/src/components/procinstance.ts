import { IProcessInstanceService } from "services/IProcessInstanceService";
import { inject } from "aurelia-framework";
import { ITaskModel } from "model/resource/ITaskModel";
import { IResources } from "model/query/IResources";
import { IProcessInstanceHistoriceService } from "services/IProcessInstanceHistoriceService";

@inject("processInstanceService", "historyService")
export class ProcessInstanceViewModel {

  instance;

  tasks: IResources<ITaskModel>;

  instanceId;

  status;

  constructor(private procInstSrv: IProcessInstanceService, private historyService: IProcessInstanceHistoriceService) {

  }

  activate(model: any, nctx) {
    this.instanceId = model.id;
    this.status = model.status;

    this.load();
  }

  back() {
    window.history.go(-1);
  }

  load() {
    this.instance = null;
    this.tasks = null;

    if (this.status == 'completed') {
      this.loadCompleted();
    } else if (this.status == 'uncompleted') {
      this.loadUnCompleted();
    }
  }

  private loadCompleted() {
    Promise.all([
      this.historyService.getProcessInstanceById(this.instanceId),
      this.procInstSrv.getTasks(this.instanceId, {
        includeCompleted: true,
        pageable: {
          pageNo: 1,
          pageSize: 1000
        }
      })
    ]).then(
      args => {
        this.instance = args[0];
        this.tasks = args[1];
      }
    );

  }

  private loadUnCompleted() {

    Promise.all([
      this.procInstSrv.getProcessInstanceById(this.instanceId),
      this.procInstSrv.getTasks(this.instanceId, {
        includeCompleted: true,
        pageable: {
          pageNo: 1,
          pageSize: 1000
        }
      })
    ]).then(
      args => {
        this.instance = args[0];
        this.tasks = args[1];
      }
    );
  }

  terminateTask(id: string) {
    debugger;
  }

  toUser(id: string) {
    debugger;
  }
}
