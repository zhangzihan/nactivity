import { Settings } from './../model/settings';
import { HttpInvoker } from 'services/httpInvoker';
import { ITaskService } from "./ITaskService";
import { ITaskQuery } from "model/query/ITaskQuery";
import { IResources } from "model/query/IResources";
import { ITaskModel } from "model/resource/ITaskModel";
import { ICompleteTaskCmd } from "model/cmd/ICompleteTaskCmd";
import { ICreateTaskCmd } from "model/cmd/ICreateTaskCmd";
import { IUpdateTaskCmd } from "model/cmd/IUpdateTaskCmd";
import contants from "../contants";
import { inject, singleton, autoinject } from "aurelia-framework";
import { ITransferTaskCmd } from 'model/cmd/ITransferTaskCmd';
import { IAppendCountersignCmd } from 'model/cmd/IAppendCountersignCmd';
import { ITerminateTaskCmd } from "model/cmd/ITerminateTaskCmd";
import { LoginUser } from 'model/loginuser';

@autoinject()
@singleton()
export class TaskService implements ITaskService {

  private controller = 'workflow/tasks';

  constructor(private httpInvoker: HttpInvoker,
    private loginUser: LoginUser,
    private settings: Settings) {

  }

  getTasks(query: ITaskQuery): Promise<IResources<ITaskModel>> {
    query.tenantId = this.loginUser.current.tenantId
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}`), query).then(data => {
        res(data.data.list);
      }).catch(err => rej(err));
    });
  }

  getTaskById(taskId: string): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  myTasks(userId: string): Promise<IResources<ITaskModel>> {
    throw new Error("Method not implemented.");
  }

  claimTask(taskId: string): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  releaseTask(taskId: string): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  completeTask(cmd: ICompleteTaskCmd): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/complete`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  deleteTask(taskId: string): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/${taskId}/remove`)).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  createNewTask(createTaskCmd: ICreateTaskCmd): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  updateTask(taskId: string, updateTaskCmd: IUpdateTaskCmd): Promise<any> {
    throw new Error("Method not implemented.");
  }

  createSubtask(taskId: string, createSubtaskCmd: ICreateTaskCmd): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  getSubtasks(taskId: string): Promise<ITaskModel> {
    throw new Error("Method not implemented.");
  }

  terminateTask(cmd: ITerminateTaskCmd): Promise<any> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/terminate`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  transferTask(cmd: ITransferTaskCmd): Promise<Array<ITaskModel>> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/transfer`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }

  appendCountersign(cmd: IAppendCountersignCmd): Promise<Array<ITaskModel>> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(this.settings.getUrl(`${this.controller}/append`), cmd).then(data => {
        res(data.data);
      }).catch(err => rej(err));
    });
  }
}
