import { IProcessInstanceService } from "services/IProcessInstanceService";
import { inject } from "aurelia-framework";
import { ITaskModel } from "model/resource/ITaskModel";
import { IResources } from "model/query/IResources";

@inject("processInstanceService")
export class ProcessInstanceViewModel {

  instance;

  tasks: IResources<ITaskModel>;

  constructor(private procInstSrv: IProcessInstanceService) {

  }

  activate(model: any, nctx) {
    Promise.all([
      this.procInstSrv.getProcessInstanceById(model.id),
      this.procInstSrv.getTasks(model.id, { pageable: { pageNo: 1, pageSize: 1000 } })
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
