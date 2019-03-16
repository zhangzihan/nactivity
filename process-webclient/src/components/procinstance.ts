import { ITaskService } from './../services/ITaskService';
import { IProcessInstanceService } from "services/IProcessInstanceService";
import { inject } from "aurelia-framework";
import { ITaskModel } from "model/resource/ITaskModel";
import { IResources } from "model/query/IResources";
import { IProcessInstanceHistoriceService } from "services/IProcessInstanceHistoriceService";

@inject("processInstanceService", "historyService", "taskService")
export class ProcessInstanceViewModel {

  instance;

  tasks: IResources<ITaskModel>;

  instanceId;

  status;

  selectedUser;

  users = [{
    id: '评审员',
    name: '评审员'
  }];

  constructor(private procInstSrv: IProcessInstanceService,
    private historyService: IProcessInstanceHistoriceService,
    private taskService: ITaskService) {

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

  selectUser(user) {
    this.selectedUser = user;
  }

  addUser() {
    this.selectedUser = {
      id: '评审员',
      name: '评审员'
    };
    this.users.push(this.selectedUser);
  }

  removeUser() {
    var idx = this.users.findIndex(x => x == this.selectedUser);
    if (idx > -1) {
      this.users.splice(idx, 1);
    }
  }

  terminateTask(id: string) {
    this.taskService.terminateTask({
      taskId: id,
      terminateReason: "已终止任务"
    })
  }

  transfer(id: string) {
    this.taskService.transferTask({
      assignees: this.users.map(x => x.id),
      taskId: id
    }).then(data => {
      debugger;
    }).catch(err => {
      alert(err);
    })
  }

  appendCountersign(id: string) {
    this.taskService.appendCountersign({
      assignees: this.users.map(x => x.id),
      taskId: id
    }).then(data => {
      debugger;
    }).catch(err => {
      alert(err);
    })
  }
}
