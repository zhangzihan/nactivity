import { inject } from "aurelia-framework";
import { IProcessInstanceService } from '../services/IProcessInstanceService'
import { DirectionEnum } from "model/query/ISort";

@inject('processInstanceService')
export class ProcInstancesViewModel {

  instances: Array<any>;

  constructor(private procInsSrv: IProcessInstanceService) {

  }

  activate(model, nctx) {
    this.procInsSrv.processInstances({
      onlyProcessInstanceExecutions:true,
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
    debugger;
  }

  terminate(id) {
    debugger;
  }
}
