import { HttpInvoker } from 'services/httpInvoker';
import { ITaskService } from "./ITaskService";
import { ITaskQuery } from "model/query/ITaskQuery";
import { IResources } from "model/query/IResources";
import { ITaskModel } from "model/resource/ITaskModel";
import { ICompleteTaskCmd } from "model/cmd/ICompleteTaskCmd";
import { ICreateTaskCmd } from "model/cmd/ICreateTaskCmd";
import { IUpdateTaskCmd } from "model/cmd/IUpdateTaskCmd";
import contants from "contants";
import { inject } from "aurelia-framework";

@inject('httpInvoker')
export class TaskService implements ITaskService {

  private controller = 'workflow/tasks';

  constructor(private httpInvoker: HttpInvoker) {

  }

  getTasks(query: ITaskQuery): Promise<IResources<ITaskModel>> {
    return new Promise((res, rej) => {
      this.httpInvoker.post(`${contants.serverUrl}/${this.controller}`, query).then(data => {
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

  completeTask(taskId: string, completeTaskCmd: ICompleteTaskCmd): Promise<any> {
    throw new Error("Method not implemented.");
  }

  deleteTask(taskId: string): Promise<any> {
    throw new Error("Method not implemented.");
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
}
